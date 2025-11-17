using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.ConfigMgr;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for computer management operations
/// </summary>
public class ComputerService : IComputerService
{
    private readonly ConfigMgrUtility _configMgrUtility;
    private readonly ILogger<ComputerService> _logger;

    public ComputerService(ConfigMgrUtility configMgrUtility, ILogger<ComputerService> logger)
    {
        _configMgrUtility = configMgrUtility ?? throw new ArgumentNullException(nameof(configMgrUtility));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> AddComputerByBiosGuidAsync(string computerName, string biosGuid)
    {
        _logger.LogInformation("Adding computer {ComputerName} by BIOS GUID {BiosGuid}",
            computerName, biosGuid);

        try
        {
            var resourceId = await _configMgrUtility.AddNewComputerBySmBiosGuidAsync(computerName, biosGuid);

            _logger.LogInformation("Successfully added computer {ComputerName} with Resource ID {ResourceId}",
                computerName, resourceId);

            return resourceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding computer {ComputerName} by BIOS GUID", computerName);
            throw;
        }
    }

    public async Task<int> AddComputerByMacAddressAsync(string computerName, string macAddress)
    {
        _logger.LogInformation("Adding computer {ComputerName} by MAC address {MacAddress}",
            computerName, macAddress);

        try
        {
            var resourceId = await _configMgrUtility.AddNewComputerByMacAddressAsync(computerName, macAddress);

            _logger.LogInformation("Successfully added computer {ComputerName} with Resource ID {ResourceId}",
                computerName, resourceId);

            return resourceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding computer {ComputerName} by MAC address", computerName);
            throw;
        }
    }

    public async Task<bool> DeleteComputerAsync(string computerName)
    {
        _logger.LogInformation("Deleting computer {ComputerName}", computerName);

        try
        {
            var deletedCount = await _configMgrUtility.DeleteComputerByNameAsync(computerName);

            var success = deletedCount > 0;

            if (success)
            {
                _logger.LogInformation("Successfully deleted computer {ComputerName}", computerName);
            }
            else
            {
                _logger.LogWarning("Computer {ComputerName} not found for deletion", computerName);
                throw new KeyNotFoundException($"Computer {computerName} not found");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting computer {ComputerName}", computerName);
            throw;
        }
    }

    public async Task<bool> CheckComputerExistsAsync(string computerName)
    {
        _logger.LogDebug("Checking if computer {ComputerName} exists", computerName);

        try
        {
            var exists = await _configMgrUtility.ClientExistsByNameAsync(computerName);

            _logger.LogDebug("Computer {ComputerName} exists: {Exists}", computerName, exists);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if computer {ComputerName} exists", computerName);
            throw;
        }
    }

    public async Task ClearPxeFlagAsync(string computerName)
    {
        _logger.LogInformation("Clearing PXE flag for computer {ComputerName}", computerName);

        try
        {
            await _configMgrUtility.ClearLastPxeAdvertisementAsync(computerName);

            _logger.LogInformation("Successfully cleared PXE flag for computer {ComputerName}", computerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing PXE flag for computer {ComputerName}", computerName);
            throw;
        }
    }

    public async Task<int> GetComputerResourceIdAsync(string computerName)
    {
        _logger.LogDebug("Getting Resource ID for computer {ComputerName}", computerName);

        try
        {
            var resourceId = await _configMgrUtility.GetComputerResourceIdByNameAsync(computerName);

            _logger.LogDebug("Computer {ComputerName} has Resource ID {ResourceId}",
                computerName, resourceId);

            return resourceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Resource ID for computer {ComputerName}", computerName);
            throw;
        }
    }

    public async Task<string> GetSmsGuidAsync(string computerName)
    {
        _logger.LogDebug("Getting SMS GUID for computer {ComputerName}", computerName);

        try
        {
            var smsGuid = await _configMgrUtility.GetSmsGuidAsync(computerName);

            _logger.LogDebug("Computer {ComputerName} has SMS GUID {SmsGuid}",
                computerName, smsGuid);

            return smsGuid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS GUID for computer {ComputerName}", computerName);
            throw;
        }
    }
}
