using Azure.Identity;
using ConfigMgrWebService.Infrastructure.Graph.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net;

namespace ConfigMgrWebService.Infrastructure.Graph;

/// <summary>
/// Utility class for Microsoft Graph API operations (Intune, Entra ID)
/// Modernized for .NET 8 with async/await and dependency injection
/// </summary>
public class GraphUtil : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GraphUtil> _logger;
    private readonly string _token;
    private bool _isDisposed;

    public string AppId { get; }
    public string AppDisplayName { get; }
    public string TenantId { get; }
    public string GraphUrl { get; }
    public string SecretString { get; }

    /// <summary>
    /// Web request methods supported by Graph API
    /// </summary>
    public enum WebMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    /// <summary>
    /// Initializes a new instance of GraphUtil with Graph API configuration
    /// </summary>
    /// <param name="appDisplayName">Application display name</param>
    /// <param name="appId">Azure AD Application (Client) ID</param>
    /// <param name="tenantId">Azure AD Tenant ID</param>
    /// <param name="graphUrl">Microsoft Graph API base URL</param>
    /// <param name="secretString">Client Secret</param>
    /// <param name="logger">Logger instance</param>
    public GraphUtil(
        string appDisplayName,
        string appId,
        string tenantId,
        string graphUrl,
        string secretString,
        ILogger<GraphUtil> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isDisposed = false;

        AppId = appId ?? throw new ArgumentNullException(nameof(appId));
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        GraphUrl = graphUrl ?? throw new ArgumentNullException(nameof(graphUrl));
        AppDisplayName = appDisplayName ?? throw new ArgumentNullException(nameof(appDisplayName));
        SecretString = secretString ?? throw new ArgumentNullException(nameof(secretString));

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(GraphUrl)
        };

        // Acquire token synchronously in constructor (will be refactored to factory pattern later)
        _token = GetAccessTokenAsync(TenantId, AppId, SecretString).GetAwaiter().GetResult();

        _logger.LogInformation("GraphUtil initialized for tenant {TenantId}", TenantId);
    }

    #region Intune Methods

    /// <summary>
    /// Gets the Intune Device ID by device name
    /// </summary>
    public async Task<string> GetIntuneDeviceIdByNameAsync(string name)
    {
        _logger.LogDebug("Getting Intune device ID for {DeviceName}", name);

        var request = GetWebRequest(WebMethod.Get, $"deviceManagement/managedDevices?$filter=(deviceName eq '{name}')");
        var content = await GetResponseAsync(request);

        var devices = JsonConvert.DeserializeObject<ManagedDeviceCollectionResponse>(content);

        if (devices?.Value != null)
        {
            foreach (var device in devices.Value)
            {
                _logger.LogDebug("Found Intune device {DeviceName} with ID {DeviceId}", name, device.Id);
                return device.Id ?? string.Empty;
            }
        }

        _logger.LogWarning("Intune device {DeviceName} not found", name);
        return string.Empty;
    }

    /// <summary>
    /// Gets the primary user of an Intune-managed device
    /// </summary>
    public async Task<string> GetPrimaryUserAsync(string deviceId)
    {
        _logger.LogDebug("Getting primary user for device {DeviceId}", deviceId);

        var request = GetWebRequest(WebMethod.Get, $"deviceManagement/managedDevices/{deviceId}/users");
        var content = await GetResponseAsync(request);

        var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);

        if (users?.Value != null)
        {
            foreach (var user in users.Value)
            {
                _logger.LogDebug("Found primary user {UserPrincipalName} for device {DeviceId}",
                    user.UserPrincipalName, deviceId);
                return user.UserPrincipalName ?? string.Empty;
            }
        }

        _logger.LogWarning("No primary user found for device {DeviceId}", deviceId);
        return string.Empty;
    }

    /// <summary>
    /// Sets the primary user for an Intune-managed device
    /// </summary>
    public async Task<bool> SetPrimaryUserAsync(string deviceId, string userId)
    {
        _logger.LogInformation("Setting primary user {UserId} for device {DeviceId}", userId, deviceId);

        var request = GetWebRequest(WebMethod.Post, $"deviceManagement/managedDevices('{deviceId}')/users/$ref");
        var body = $"{{ \"@odata.id\": \"{GraphUrl}users/{userId}\"}}";

        var encoding = new System.Text.ASCIIEncoding();
        byte[] data = encoding.GetBytes(body);
        request.ContentLength = data.Length;

        using (var stream = await request.GetRequestStreamAsync())
        {
            await stream.WriteAsync(data, 0, data.Length);
        }

        var content = await GetResponseAsync(request);
        var success = string.IsNullOrEmpty(content);

        if (success)
        {
            _logger.LogInformation("Successfully set primary user {UserId} for device {DeviceId}", userId, deviceId);
        }
        else
        {
            _logger.LogWarning("Failed to set primary user {UserId} for device {DeviceId}", userId, deviceId);
        }

        return success;
    }

    /// <summary>
    /// Checks if a device is co-managed (ConfigMgr + Intune)
    /// </summary>
    public async Task<bool> IsDeviceCoManagedAsync(string deviceId)
    {
        _logger.LogDebug("Checking if device {DeviceId} is co-managed", deviceId);

        var request = GetWebRequest(
            WebMethod.Get,
            $"deviceManagement/managedDevices/{deviceId}?$select=id,managementAgent,deviceEnrollmentType"
        );

        var content = await GetResponseAsync(request);
        var device = JsonConvert.DeserializeObject<ManagedDevice>(content);

        if (device != null)
        {
            // Check if device is co-managed
            if (device.ManagementAgent == ManagementAgentType.ConfigurationManagerClientMdm)
            {
                _logger.LogDebug("Device {DeviceId} is co-managed (ConfigMgr+MDM)", deviceId);
                return true;
            }

            if (device.DeviceEnrollmentType == DeviceEnrollmentType.WindowsCoManagement)
            {
                _logger.LogDebug("Device {DeviceId} is co-managed (Windows CoManagement)", deviceId);
                return true;
            }
        }

        _logger.LogDebug("Device {DeviceId} is not co-managed", deviceId);
        return false;
    }

    /// <summary>
    /// Sets the device category for an Intune-managed device
    /// </summary>
    public async Task<bool> SetDeviceCategoryAsync(string deviceId, string deviceCategoryName)
    {
        _logger.LogInformation("Setting device category {CategoryName} for device {DeviceId}",
            deviceCategoryName, deviceId);

        // 1. Get category ID by name
        var catRequest = GetWebRequest(WebMethod.Get, "deviceManagement/deviceCategories?$select=id,displayName");
        var catContent = await GetResponseAsync(catRequest);

        var catColl = JsonConvert.DeserializeObject<DeviceCategoryCollectionResponse>(catContent);
        var category = catColl?.Value?.FirstOrDefault(c =>
            c.DisplayName != null && c.DisplayName.Equals(deviceCategoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            _logger.LogError("Device category {CategoryName} not found in Intune", deviceCategoryName);
            throw new InvalidOperationException($"Device Category '{deviceCategoryName}' not found in Intune!");
        }

        // 2. Set device category
        var putEndpoint = $"deviceManagement/managedDevices/{deviceId}/deviceCategory/$ref";
        var putRequest = GetWebRequest(WebMethod.Put, putEndpoint);

        var body = $"{{ \"@odata.id\": \"{GraphUrl}deviceManagement/deviceCategories/{category.Id}\"}}";
        var encoding = new System.Text.ASCIIEncoding();
        byte[] data = encoding.GetBytes(body);
        putRequest.ContentLength = data.Length;

        try
        {
            using (var stream = await putRequest.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            var putContent = await GetResponseAsync(putRequest);
            var success = string.IsNullOrEmpty(putContent);

            if (success)
            {
                _logger.LogInformation("Successfully set device category {CategoryName} for device {DeviceId}",
                    deviceCategoryName, deviceId);
            }

            return success;
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse errorResponse &&
                errorResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("Device {DeviceId} or category {CategoryName} not found",
                    deviceId, deviceCategoryName);
                return false;
            }

            _logger.LogError(ex, "Error setting device category {CategoryName} for device {DeviceId}",
                deviceCategoryName, deviceId);
            throw;
        }
    }

    #endregion

    #region Entra ID Methods

    /// <summary>
    /// Gets user ID by User Principal Name (UPN)
    /// </summary>
    public async Task<string> GetUserIdByUpnAsync(string userPrincipalName)
    {
        _logger.LogDebug("Getting user ID for UPN {UserPrincipalName}", userPrincipalName);

        var request = GetWebRequest(WebMethod.Get, $"users?$filter=(userPrincipalName eq '{userPrincipalName}')");
        var content = await GetResponseAsync(request);

        var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);

        if (users?.Value != null)
        {
            foreach (var user in users.Value)
            {
                _logger.LogDebug("Found user {UserPrincipalName} with ID {UserId}",
                    userPrincipalName, user.Id);
                return user.Id ?? string.Empty;
            }
        }

        _logger.LogWarning("User {UserPrincipalName} not found", userPrincipalName);
        return string.Empty;
    }

    /// <summary>
    /// Gets user ID by sAMAccountName (on-premises username)
    /// </summary>
    public async Task<string> GetUserIdBySamAccountNameAsync(string samAccountName)
    {
        _logger.LogDebug("Getting user ID for sAMAccountName {SamAccountName}", samAccountName);

        var request = GetWebRequest(
            WebMethod.Get,
            $"users?$filter=onPremisesSamAccountName eq '{samAccountName}'&$count=true",
            advancedQuery: true);

        var content = await GetResponseAsync(request);

        var users = JsonConvert.DeserializeObject<UserCollectionResponse>(content);

        if (users?.Value != null)
        {
            foreach (var user in users.Value)
            {
                _logger.LogDebug("Found user {SamAccountName} with ID {UserId}",
                    samAccountName, user.Id);
                return user.Id ?? string.Empty;
            }
        }

        _logger.LogWarning("User {SamAccountName} not found", samAccountName);
        return string.Empty;
    }

    /// <summary>
    /// Gets Entra ID device ID by device name
    /// </summary>
    public async Task<string> GetEntraDeviceIdByNameAsync(string name)
    {
        _logger.LogDebug("Getting Entra ID device ID for {DeviceName}", name);

        var request = GetWebRequest(WebMethod.Get, $"devices?$filter=displayName eq '{Uri.EscapeDataString(name)}'&$select=id");
        var content = await GetResponseAsync(request);

        var devices = JsonConvert.DeserializeObject<DeviceCollectionResponse>(content);

        if (devices?.Value != null)
        {
            foreach (var device in devices.Value)
            {
                _logger.LogDebug("Found Entra device {DeviceName} with ID {DeviceId}", name, device.Id);
                return device.Id ?? string.Empty;
            }
        }

        _logger.LogWarning("Entra device {DeviceName} not found", name);
        return string.Empty;
    }

    /// <summary>
    /// Gets Entra ID group ID by group name
    /// </summary>
    public async Task<string> GetGroupIdByNameAsync(string groupName)
    {
        _logger.LogDebug("Getting group ID for {GroupName}", groupName);

        var request = GetWebRequest(WebMethod.Get, $"groups?$filter=displayName eq '{Uri.EscapeDataString(groupName)}'&$select=id");
        var content = await GetResponseAsync(request);

        var groups = JsonConvert.DeserializeObject<GroupCollectionResponse>(content);

        if (groups?.Value != null)
        {
            foreach (var group in groups.Value)
            {
                _logger.LogDebug("Found group {GroupName} with ID {GroupId}", groupName, group.Id);
                return group.Id ?? string.Empty;
            }
        }

        _logger.LogWarning("Group {GroupName} not found", groupName);
        return string.Empty;
    }

    /// <summary>
    /// Checks if a device is a member of a group
    /// </summary>
    public async Task<bool> IsDeviceMemberOfGroupAsync(string deviceId, string groupId)
    {
        _logger.LogDebug("Checking if device {DeviceId} is member of group {GroupId}", deviceId, groupId);

        var request = GetWebRequest(WebMethod.Get, $"groups/{groupId}/members?$select=id");
        var content = await GetResponseAsync(request);

        var members = JsonConvert.DeserializeObject<DirectoryObjectCollectionResponse>(content);
        var isMember = members?.Value?.Any(m => m.Id == deviceId) ?? false;

        _logger.LogDebug("Device {DeviceId} is {MembershipStatus} of group {GroupId}",
            deviceId, isMember ? "member" : "not member", groupId);

        return isMember;
    }

    /// <summary>
    /// Adds a device to an Entra ID group
    /// </summary>
    public async Task<bool> AddDeviceToGroupAsync(string deviceId, string groupId)
    {
        _logger.LogInformation("Adding device {DeviceId} to group {GroupId}", deviceId, groupId);

        var request = GetWebRequest(WebMethod.Post, $"groups/{groupId}/members/$ref");
        var body = $"{{ \"@odata.id\": \"{GraphUrl}directoryObjects/{deviceId}\"}}";

        var encoding = new System.Text.ASCIIEncoding();
        byte[] data = encoding.GetBytes(body);
        request.ContentLength = data.Length;

        using (var stream = await request.GetRequestStreamAsync())
        {
            await stream.WriteAsync(data, 0, data.Length);
        }

        var content = await GetResponseAsync(request);
        var success = string.IsNullOrEmpty(content);

        if (success)
        {
            _logger.LogInformation("Successfully added device {DeviceId} to group {GroupId}", deviceId, groupId);
        }
        else
        {
            _logger.LogWarning("Failed to add device {DeviceId} to group {GroupId}", deviceId, groupId);
        }

        return success;
    }

    /// <summary>
    /// Removes a device from an Entra ID group
    /// </summary>
    public async Task<bool> RemoveDeviceFromGroupAsync(string deviceId, string groupId)
    {
        _logger.LogInformation("Removing device {DeviceId} from group {GroupId}", deviceId, groupId);

        var request = GetWebRequest(WebMethod.Delete, $"groups/{groupId}/members/{deviceId}/$ref");

        try
        {
            var content = await GetResponseAsync(request);
            var success = string.IsNullOrEmpty(content);

            if (success)
            {
                _logger.LogInformation("Successfully removed device {DeviceId} from group {GroupId}",
                    deviceId, groupId);
            }

            return success;
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse errorResponse &&
                errorResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Device {DeviceId} not found in group {GroupId}", deviceId, groupId);
                return false;
            }

            _logger.LogError(ex, "Error removing device {DeviceId} from group {GroupId}", deviceId, groupId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets the response content from an HTTP request
    /// </summary>
    private async Task<string> GetResponseAsync(HttpWebRequest request)
    {
        using var response = await request.GetResponseAsync();
        using var responseStream = response.GetResponseStream();
        using var streamReader = new StreamReader(responseStream, true);
        return await streamReader.ReadToEndAsync();
    }

    /// <summary>
    /// Creates an HTTP web request with authentication
    /// </summary>
    private HttpWebRequest GetWebRequest(WebMethod webMethod, string urlSubString, bool advancedQuery = false)
    {
        var url = GraphUrl + urlSubString;
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.PreAuthenticate = true;

        switch (webMethod)
        {
            case WebMethod.Get:
                request.Method = "GET";
                request.Accept = "application/json";
                break;
            case WebMethod.Post:
                request.Method = "POST";
                request.ContentType = "application/json";
                break;
            case WebMethod.Put:
                request.Method = "PUT";
                request.ContentType = "application/json";
                break;
            case WebMethod.Delete:
                request.Method = "DELETE";
                break;
            default:
                throw new ArgumentException($"Unsupported web method: {webMethod}");
        }

        request.Headers.Add("Authorization", $"Bearer {_token}");

        if (advancedQuery)
        {
            request.Headers.Add("ConsistencyLevel", "eventual");
        }

        return request;
    }

    /// <summary>
    /// Acquires an access token for Microsoft Graph API using client credentials flow
    /// </summary>
    private async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret)
    {
        _logger.LogDebug("Acquiring access token for tenant {TenantId}", tenantId);

        var builder = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithTenantId(tenantId)
            .WithRedirectUri("http://localhost/")
            .Build();

        var acquiredTokenResult = builder.AcquireTokenForClient(
            new List<string> { "https://graph.microsoft.com/.default" });

        var tokenResult = await acquiredTokenResult.ExecuteAsync();

        _logger.LogInformation("Successfully acquired access token for tenant {TenantId}", tenantId);

        return tokenResult.AccessToken;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _isDisposed = true;
        }
    }

    ~GraphUtil()
    {
        Dispose(false);
    }

    #endregion
}
