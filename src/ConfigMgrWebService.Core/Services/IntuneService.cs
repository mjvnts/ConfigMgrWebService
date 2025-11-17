using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.Graph;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for Intune operations
/// </summary>
public class IntuneService : IIntuneService
{
    private readonly GraphUtil _graphUtil;
    private readonly ILogger<IntuneService> _logger;

    public IntuneService(GraphUtil graphUtil, ILogger<IntuneService> logger)
    {
        _graphUtil = graphUtil ?? throw new ArgumentNullException(nameof(graphUtil));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetIntuneDeviceIdByNameAsync(string deviceName)
    {
        _logger.LogDebug("Getting Intune device ID for {DeviceName}", deviceName);

        try
        {
            var deviceId = await _graphUtil.GetIntuneDeviceIdByNameAsync(deviceName);

            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogWarning("Intune device {DeviceName} not found", deviceName);
                throw new KeyNotFoundException($"Intune device {deviceName} not found");
            }

            _logger.LogDebug("Intune device {DeviceName} has ID {DeviceId}", deviceName, deviceId);

            return deviceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Intune device ID for {DeviceName}", deviceName);
            throw;
        }
    }

    public async Task<string> GetPrimaryUserAsync(string deviceId)
    {
        _logger.LogDebug("Getting primary user for Intune device {DeviceId}", deviceId);

        try
        {
            var primaryUser = await _graphUtil.GetPrimaryUserAsync(deviceId);

            _logger.LogDebug("Intune device {DeviceId} has primary user {PrimaryUser}",
                deviceId, primaryUser);

            return primaryUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary user for Intune device {DeviceId}", deviceId);
            throw;
        }
    }

    public async Task<bool> SetPrimaryUserAsync(string deviceId, string userId)
    {
        _logger.LogInformation("Setting primary user {UserId} for Intune device {DeviceId}",
            userId, deviceId);

        try
        {
            var success = await _graphUtil.SetPrimaryUserAsync(deviceId, userId);

            if (success)
            {
                _logger.LogInformation("Successfully set primary user {UserId} for Intune device {DeviceId}",
                    userId, deviceId);
            }
            else
            {
                _logger.LogWarning("Failed to set primary user {UserId} for Intune device {DeviceId}",
                    userId, deviceId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary user {UserId} for Intune device {DeviceId}",
                userId, deviceId);
            throw;
        }
    }

    public async Task<bool> IsDeviceCoManagedAsync(string deviceId)
    {
        _logger.LogDebug("Checking if Intune device {DeviceId} is co-managed", deviceId);

        try
        {
            var isCoManaged = await _graphUtil.IsDeviceCoManagedAsync(deviceId);

            _logger.LogDebug("Intune device {DeviceId} is {CoManagedStatus}",
                deviceId, isCoManaged ? "co-managed" : "not co-managed");

            return isCoManaged;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if Intune device {DeviceId} is co-managed", deviceId);
            throw;
        }
    }

    public async Task<bool> SetDeviceCategoryAsync(string deviceId, string categoryName)
    {
        _logger.LogInformation("Setting device category {CategoryName} for Intune device {DeviceId}",
            categoryName, deviceId);

        try
        {
            var success = await _graphUtil.SetDeviceCategoryAsync(deviceId, categoryName);

            if (success)
            {
                _logger.LogInformation("Successfully set device category {CategoryName} for Intune device {DeviceId}",
                    categoryName, deviceId);
            }
            else
            {
                _logger.LogWarning("Failed to set device category {CategoryName} for Intune device {DeviceId}",
                    categoryName, deviceId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting device category {CategoryName} for Intune device {DeviceId}",
                categoryName, deviceId);
            throw;
        }
    }
}
