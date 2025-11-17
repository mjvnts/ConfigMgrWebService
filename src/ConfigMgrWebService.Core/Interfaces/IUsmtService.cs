namespace ConfigMgrWebService.Core.Interfaces;

/// <summary>
/// Service interface for USMT (User State Migration Tool) operations
/// </summary>
public interface IUsmtService
{
    /// <summary>
    /// Creates a computer association for USMT
    /// </summary>
    Task<bool> CreateAssociationAsync(string sourceComputer, string destinationComputer);

    /// <summary>
    /// Removes a computer association
    /// </summary>
    Task<bool> RemoveAssociationAsync(string sourceComputer, string destinationComputer);

    /// <summary>
    /// Gets migration status for an association
    /// </summary>
    Task<string> GetMigrationStatusAsync(string sourceComputer, string destinationComputer);
}
