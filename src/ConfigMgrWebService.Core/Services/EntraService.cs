using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.Graph;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for Entra ID (Azure AD) operations
/// </summary>
public class EntraService : IEntraService
{
    private readonly GraphUtil _graphUtil;
    private readonly ILogger<EntraService> _logger;

    public EntraService(GraphUtil graphUtil, ILogger<EntraService> logger)
    {
        _graphUtil = graphUtil ?? throw new ArgumentNullException(nameof(graphUtil));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetUserIdByUpnAsync(string userPrincipalName)
    {
        _logger.LogDebug("Getting user ID for UPN {UserPrincipalName}", userPrincipalName);

        try
        {
            var userId = await _graphUtil.GetUserIdByUpnAsync(userPrincipalName);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User {UserPrincipalName} not found", userPrincipalName);
                throw new KeyNotFoundException($"User {userPrincipalName} not found");
            }

            _logger.LogDebug("User {UserPrincipalName} has ID {UserId}", userPrincipalName, userId);

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID for UPN {UserPrincipalName}", userPrincipalName);
            throw;
        }
    }

    public async Task<string> GetUserIdBySamAccountNameAsync(string samAccountName)
    {
        _logger.LogDebug("Getting user ID for sAMAccountName {SamAccountName}", samAccountName);

        try
        {
            var userId = await _graphUtil.GetUserIdBySamAccountNameAsync(samAccountName);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User {SamAccountName} not found", samAccountName);
                throw new KeyNotFoundException($"User {samAccountName} not found");
            }

            _logger.LogDebug("User {SamAccountName} has ID {UserId}", samAccountName, userId);

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID for sAMAccountName {SamAccountName}", samAccountName);
            throw;
        }
    }

    public async Task<string> GetEntraDeviceIdByNameAsync(string deviceName)
    {
        _logger.LogDebug("Getting Entra device ID for {DeviceName}", deviceName);

        try
        {
            var deviceId = await _graphUtil.GetEntraDeviceIdByNameAsync(deviceName);

            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogWarning("Entra device {DeviceName} not found", deviceName);
                throw new KeyNotFoundException($"Entra device {deviceName} not found");
            }

            _logger.LogDebug("Entra device {DeviceName} has ID {DeviceId}", deviceName, deviceId);

            return deviceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Entra device ID for {DeviceName}", deviceName);
            throw;
        }
    }

    public async Task<string> GetGroupIdByNameAsync(string groupName)
    {
        _logger.LogDebug("Getting group ID for {GroupName}", groupName);

        try
        {
            var groupId = await _graphUtil.GetGroupIdByNameAsync(groupName);

            if (string.IsNullOrEmpty(groupId))
            {
                _logger.LogWarning("Group {GroupName} not found", groupName);
                throw new KeyNotFoundException($"Group {groupName} not found");
            }

            _logger.LogDebug("Group {GroupName} has ID {GroupId}", groupName, groupId);

            return groupId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group ID for {GroupName}", groupName);
            throw;
        }
    }

    public async Task<bool> IsDeviceMemberOfGroupAsync(string deviceId, string groupId)
    {
        _logger.LogDebug("Checking if device {DeviceId} is member of group {GroupId}",
            deviceId, groupId);

        try
        {
            var isMember = await _graphUtil.IsDeviceMemberOfGroupAsync(deviceId, groupId);

            _logger.LogDebug("Device {DeviceId} is {MembershipStatus} of group {GroupId}",
                deviceId, isMember ? "member" : "not member", groupId);

            return isMember;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if device {DeviceId} is member of group {GroupId}",
                deviceId, groupId);
            throw;
        }
    }

    public async Task<bool> AddDeviceToGroupAsync(string deviceId, string groupId)
    {
        _logger.LogInformation("Adding device {DeviceId} to group {GroupId}", deviceId, groupId);

        try
        {
            var success = await _graphUtil.AddDeviceToGroupAsync(deviceId, groupId);

            if (success)
            {
                _logger.LogInformation("Successfully added device {DeviceId} to group {GroupId}",
                    deviceId, groupId);
            }
            else
            {
                _logger.LogWarning("Failed to add device {DeviceId} to group {GroupId}",
                    deviceId, groupId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding device {DeviceId} to group {GroupId}", deviceId, groupId);
            throw;
        }
    }

    public async Task<bool> RemoveDeviceFromGroupAsync(string deviceId, string groupId)
    {
        _logger.LogInformation("Removing device {DeviceId} from group {GroupId}", deviceId, groupId);

        try
        {
            var success = await _graphUtil.RemoveDeviceFromGroupAsync(deviceId, groupId);

            if (success)
            {
                _logger.LogInformation("Successfully removed device {DeviceId} from group {GroupId}",
                    deviceId, groupId);
            }
            else
            {
                _logger.LogWarning("Failed to remove device {DeviceId} from group {GroupId}",
                    deviceId, groupId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device {DeviceId} from group {GroupId}", deviceId, groupId);
            throw;
        }
    }
}
