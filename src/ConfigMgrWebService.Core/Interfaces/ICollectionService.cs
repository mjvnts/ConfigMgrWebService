namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for collection management operations
/// </summary>
public interface ICollectionService
{
    /// <summary>
    /// Creates a new device collection
    /// </summary>
    Task<string> CreateDeviceCollectionAsync(
        string collectionName,
        string collectionDescription,
        string limitingCollection);

    /// <summary>
    /// Gets collection ID by name
    /// </summary>
    Task<string> GetCollectionIdByNameAsync(string collectionName);

    /// <summary>
    /// Gets all members of a collection
    /// </summary>
    Task<List<string>> GetCollectionMembersAsync(string collectionId);

    /// <summary>
    /// Adds a computer to a collection
    /// </summary>
    Task AddMemberToCollectionAsync(string collectionId, string computerName);

    /// <summary>
    /// Removes a computer from a collection
    /// </summary>
    Task RemoveMemberFromCollectionAsync(string collectionId, string computerName);

    /// <summary>
    /// Updates collection membership
    /// </summary>
    Task UpdateCollectionMembershipAsync(string collectionId);
}
