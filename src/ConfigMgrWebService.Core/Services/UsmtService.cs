using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.ConfigMgr;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for USMT (User State Migration Tool) operations
/// </summary>
public class UsmtService : IUsmtService
{
    private readonly ConfigMgrUtility _configMgrUtility;
    private readonly ILogger<UsmtService> _logger;

    public UsmtService(ConfigMgrUtility configMgrUtility, ILogger<UsmtService> logger)
    {
        _configMgrUtility = configMgrUtility ?? throw new ArgumentNullException(nameof(configMgrUtility));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> CreateAssociationAsync(string sourceComputer, string destinationComputer)
    {
        _logger.LogInformation("Creating USMT association from {SourceComputer} to {DestinationComputer}",
            sourceComputer, destinationComputer);

        try
        {
            var success = await _configMgrUtility.AddUsmtComputerAssociationAsync(
                sourceComputer,
                destinationComputer);

            _logger.LogInformation("Successfully created USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputer, destinationComputer);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputer, destinationComputer);
            throw;
        }
    }

    public async Task<bool> RemoveAssociationAsync(string sourceComputer, string destinationComputer)
    {
        _logger.LogInformation("Removing USMT association from {SourceComputer} to {DestinationComputer}",
            sourceComputer, destinationComputer);

        try
        {
            var success = await _configMgrUtility.RemoveUsmtComputerAssociationAsync(
                sourceComputer,
                destinationComputer);

            _logger.LogInformation("Successfully removed USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputer, destinationComputer);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputer, destinationComputer);
            throw;
        }
    }

    public async Task<string> GetMigrationStatusAsync(string sourceComputer, string destinationComputer)
    {
        _logger.LogDebug("Getting USMT migration status from {SourceComputer} to {DestinationComputer}",
            sourceComputer, destinationComputer);

        try
        {
            var status = await _configMgrUtility.GetUsmtMigrationStatusAsync(
                sourceComputer,
                destinationComputer);

            _logger.LogDebug("USMT migration status from {SourceComputer} to {DestinationComputer}: {Status}",
                sourceComputer, destinationComputer, status);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting USMT migration status from {SourceComputer} to {DestinationComputer}",
                sourceComputer, destinationComputer);
            throw;
        }
    }
}
