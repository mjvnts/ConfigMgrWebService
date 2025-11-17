namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for Entra ID (Azure AD) operations
/// </summary>
public interface IEntraService
{
    /// <summary>
    /// Gets user ID by User Principal Name
    /// </summary>
    Task<string> GetUserIdByUpnAsync(string userPrincipalName);

    /// <summary>
    /// Gets user ID by sAMAccountName
    /// </summary>
    Task<string> GetUserIdBySamAccountNameAsync(string samAccountName);

    /// <summary>
    /// Gets Entra device ID by device name
    /// </summary>
    Task<string> GetEntraDeviceIdByNameAsync(string deviceName);

    /// <summary>
    /// Gets group ID by group name
    /// </summary>
    Task<string> GetGroupIdByNameAsync(string groupName);

    /// <summary>
    /// Checks if a device is member of a group
    /// </summary>
    Task<bool> IsDeviceMemberOfGroupAsync(string deviceId, string groupId);

    /// <summary>
    /// Adds a device to a group
    /// </summary>
    Task<bool> AddDeviceToGroupAsync(string deviceId, string groupId);

    /// <summary>
    /// Removes a device from a group
    /// </summary>
    Task<bool> RemoveDeviceFromGroupAsync(string deviceId, string groupId);
}
