using ConfigMgrWebService.Core.Interfaces;
using ConfigMgrWebService.Infrastructure.ConfigMgr;
using Microsoft.Extensions.Logging;

namespace ConfigMgrWebService.Core.Services;

/// <summary>
/// Service for collection management operations
/// </summary>
public class CollectionService : ICollectionService
{
    private readonly ConfigMgrUtility _configMgrUtility;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(ConfigMgrUtility configMgrUtility, ILogger<CollectionService> logger)
    {
        _configMgrUtility = configMgrUtility ?? throw new ArgumentNullException(nameof(configMgrUtility));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CreateDeviceCollectionAsync(
        string collectionName,
        string collectionDescription,
        string limitingCollection)
    {
        _logger.LogInformation("Creating device collection {CollectionName}", collectionName);

        try
        {
            var collectionId = await _configMgrUtility.CreateDeviceCollectionAsync(
                collectionName,
                collectionDescription,
                limitingCollection);

            _logger.LogInformation("Successfully created device collection {CollectionName} with ID {CollectionId}",
                collectionName, collectionId);

            return collectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating device collection {CollectionName}", collectionName);
            throw;
        }
    }

    public async Task<string> GetCollectionIdByNameAsync(string collectionName)
    {
        _logger.LogDebug("Getting collection ID for {CollectionName}", collectionName);

        try
        {
            var collectionId = await _configMgrUtility.GetCollectionIdByNameAsync(collectionName);

            _logger.LogDebug("Collection {CollectionName} has ID {CollectionId}",
                collectionName, collectionId);

            return collectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection ID for {CollectionName}", collectionName);
            throw;
        }
    }

    public async Task<List<string>> GetCollectionMembersAsync(string collectionId)
    {
        _logger.LogDebug("Getting members of collection {CollectionId}", collectionId);

        try
        {
            var members = await _configMgrUtility.GetMembersOfCollectionAsync(collectionId);

            _logger.LogDebug("Collection {CollectionId} has {MemberCount} members",
                collectionId, members.Count);

            return members;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members of collection {CollectionId}", collectionId);
            throw;
        }
    }

    public async Task AddMemberToCollectionAsync(string collectionId, string computerName)
    {
        _logger.LogInformation("Adding {ComputerName} to collection {CollectionId}",
            computerName, collectionId);

        try
        {
            await _configMgrUtility.AddMemberToCollectionDirectByNameAsync(collectionId, computerName);

            _logger.LogInformation("Successfully added {ComputerName} to collection {CollectionId}",
                computerName, collectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {ComputerName} to collection {CollectionId}",
                computerName, collectionId);
            throw;
        }
    }

    public async Task RemoveMemberFromCollectionAsync(string collectionId, string computerName)
    {
        _logger.LogInformation("Removing {ComputerName} from collection {CollectionId}",
            computerName, collectionId);

        try
        {
            await _configMgrUtility.RemoveMemberFromCollectionDirectByNameAsync(collectionId, computerName);

            _logger.LogInformation("Successfully removed {ComputerName} from collection {CollectionId}",
                computerName, collectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing {ComputerName} from collection {CollectionId}",
                computerName, collectionId);
            throw;
        }
    }

    public async Task UpdateCollectionMembershipAsync(string collectionId)
    {
        _logger.LogInformation("Updating membership for collection {CollectionId}", collectionId);

        try
        {
            await _configMgrUtility.UpdateCollectionMembershipAsync(collectionId);

            _logger.LogInformation("Successfully updated membership for collection {CollectionId}",
                collectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating membership for collection {CollectionId}", collectionId);
            throw;
        }
    }
}
