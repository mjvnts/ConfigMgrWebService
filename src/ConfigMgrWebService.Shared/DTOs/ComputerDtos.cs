using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to add a new computer by BIOS GUID
/// </summary>
public class AddComputerByBiosGuidRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Computer name must be between 1 and 255 characters")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "BIOS GUID is required")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Invalid BIOS GUID format")]
    public string BiosGuid { get; set; } = string.Empty;
}

/// <summary>
/// Request to delete a computer
/// </summary>
public class DeleteComputerRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to delete a computer by GUID
/// </summary>
public class DeleteComputerByGuidRequest
{
    [Required(ErrorMessage = "GUID is required")]
    [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        ErrorMessage = "Invalid GUID format")]
    public string Guid { get; set; } = string.Empty;
}

/// <summary>
/// Request to check if computer exists
/// </summary>
public class CheckComputerExistsRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Response with computer existence status
/// </summary>
public class ComputerExistsResponse
{
    public bool Exists { get; set; }
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to clear PXE flag
/// </summary>
public class ClearPxeFlagRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to add computer variable
/// </summary>
public class AddComputerVariableRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Variable name is required")]
    public string VariableName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Variable value is required")]
    public string VariableValue { get; set; } = string.Empty;
}

/// <summary>
/// Request to delete computer variable
/// </summary>
public class DeleteComputerVariableRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Variable name is required")]
    public string VariableName { get; set; } = string.Empty;
}

/// <summary>
/// Request to replace all computer variables
/// </summary>
public class ReplaceAllComputerVariablesRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Variables are required")]
    public Dictionary<string, string> Variables { get; set; } = new();
}
