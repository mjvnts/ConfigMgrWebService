using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.ConfigMgr;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly ConfigMgrUtility _configMgrUtility;
    private readonly ILogger<UserService> _logger;

    public UserService(ConfigMgrUtility configMgrUtility, ILogger<UserService> logger)
    {
        _configMgrUtility = configMgrUtility ?? throw new ArgumentNullException(nameof(configMgrUtility));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string[]> GetPrimaryUsersAsync(string computerName)
    {
        _logger.LogDebug("Getting primary users for computer {ComputerName}", computerName);

        try
        {
            var users = await _configMgrUtility.GetPrimaryUsersAsync(computerName);

            _logger.LogDebug("Computer {ComputerName} has {UserCount} primary user(s)",
                computerName, users.Length);

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary users for computer {ComputerName}", computerName);
            throw;
        }
    }

    public async Task SetPrimaryUserAsync(string computerName, string userName)
    {
        _logger.LogInformation("Setting primary user {UserName} for computer {ComputerName}",
            userName, computerName);

        try
        {
            await _configMgrUtility.SetPrimaryUserAsync(
                computerName,
                userName,
                ConfigMgrUtility.DeviceAffinityTypes.Administrator);

            _logger.LogInformation("Successfully set primary user {UserName} for computer {ComputerName}",
                userName, computerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary user {UserName} for computer {ComputerName}",
                userName, computerName);
            throw;
        }
    }

    public async Task DeletePrimaryUserAsync(string computerName, string userName)
    {
        _logger.LogInformation("Deleting primary user {UserName} for computer {ComputerName}",
            userName, computerName);

        try
        {
            await _configMgrUtility.DeletePrimaryUserAsync(computerName, userName);

            _logger.LogInformation("Successfully deleted primary user {UserName} for computer {ComputerName}",
                userName, computerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting primary user {UserName} for computer {ComputerName}",
                userName, computerName);
            throw;
        }
    }
}
