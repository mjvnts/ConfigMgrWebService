namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for computer management operations
/// </summary>
public interface IComputerService
{
    /// <summary>
    /// Adds a new computer to ConfigMgr by BIOS GUID
    /// </summary>
    Task<int> AddComputerByBiosGuidAsync(string computerName, string biosGuid);

    /// <summary>
    /// Adds a new computer to ConfigMgr by MAC Address
    /// </summary>
    Task<int> AddComputerByMacAddressAsync(string computerName, string macAddress);

    /// <summary>
    /// Deletes a computer from ConfigMgr
    /// </summary>
    Task<bool> DeleteComputerAsync(string computerName);

    /// <summary>
    /// Checks if a computer exists in ConfigMgr
    /// </summary>
    Task<bool> CheckComputerExistsAsync(string computerName);

    /// <summary>
    /// Clears the PXE flag for a computer
    /// </summary>
    Task ClearPxeFlagAsync(string computerName);

    /// <summary>
    /// Gets the ConfigMgr Resource ID for a computer
    /// </summary>
    Task<int> GetComputerResourceIdAsync(string computerName);

    /// <summary>
    /// Gets the SMS GUID for a computer
    /// </summary>
    Task<string> GetSmsGuidAsync(string computerName);
}
