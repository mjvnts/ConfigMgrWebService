using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to create USMT computer association
/// </summary>
public class AddUsmtAssociationRequest
{
    [Required(ErrorMessage = "Source computer name is required")]
    public string SourceComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination computer name is required")]
    public string DestinationComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove USMT computer association
/// </summary>
public class RemoveUsmtAssociationRequest
{
    [Required(ErrorMessage = "Source computer name is required")]
    public string SourceComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination computer name is required")]
    public string DestinationComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to get USMT migration status
/// </summary>
public class GetUsmtMigrationStatusRequest
{
    [Required(ErrorMessage = "Source computer name is required")]
    public string SourceComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination computer name is required")]
    public string DestinationComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Response with USMT migration status
/// </summary>
public class UsmtMigrationStatusResponse
{
    public string Status { get; set; } = string.Empty; // NOTSTARTED, INPROGRESS, COMPLETED
    public string SourceComputerName { get; set; } = string.Empty;
    public string DestinationComputerName { get; set; } = string.Empty;
}
