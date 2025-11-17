using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to add computer to Entra group
/// </summary>
public class AddComputerToEntraGroupRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Group name is required")]
    public string GroupName { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove computer from Entra group
/// </summary>
public class RemoveComputerFromEntraGroupRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Group name is required")]
    public string GroupName { get; set; } = string.Empty;
}

/// <summary>
/// Request to add user to Entra group
/// </summary>
public class AddUserToEntraGroupRequest
{
    [Required(ErrorMessage = "SAM account name is required")]
    public string SamAccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Group name is required")]
    public string GroupName { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove user from Entra group
/// </summary>
public class RemoveUserFromEntraGroupRequest
{
    [Required(ErrorMessage = "SAM account name is required")]
    public string SamAccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Group name is required")]
    public string GroupName { get; set; } = string.Empty;
}
