using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to send SCCM status message
/// </summary>
public class SendSccmStatusMessageRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity is required")]
    [RegularExpression("^(Information|Warning|Error)$", ErrorMessage = "Severity must be Information, Warning, or Error")]
    public string Severity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Component is required")]
    public string Component { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;
}
