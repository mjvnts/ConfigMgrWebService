namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets primary users for a computer
    /// </summary>
    Task<string[]> GetPrimaryUsersAsync(string computerName);

    /// <summary>
    /// Sets the primary user for a computer
    /// </summary>
    Task SetPrimaryUserAsync(string computerName, string userName);

    /// <summary>
    /// Deletes the primary user for a computer
    /// </summary>
    Task DeletePrimaryUserAsync(string computerName, string userName);
}
