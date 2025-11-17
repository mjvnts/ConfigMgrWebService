namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for Intune operations
/// </summary>
public interface IIntuneService
{
    /// <summary>
    /// Gets Intune device ID by device name
    /// </summary>
    Task<string> GetIntuneDeviceIdByNameAsync(string deviceName);

    /// <summary>
    /// Gets the primary user of an Intune device
    /// </summary>
    Task<string> GetPrimaryUserAsync(string deviceId);

    /// <summary>
    /// Sets the primary user for an Intune device
    /// </summary>
    Task<bool> SetPrimaryUserAsync(string deviceId, string userId);

    /// <summary>
    /// Checks if a device is co-managed
    /// </summary>
    Task<bool> IsDeviceCoManagedAsync(string deviceId);

    /// <summary>
    /// Sets device category for an Intune device
    /// </summary>
    Task<bool> SetDeviceCategoryAsync(string deviceId, string categoryName);
}
