using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to add primary user
/// </summary>
public class AddPrimaryUserRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove primary user
/// </summary>
public class RemovePrimaryUserRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// Request to get primary users
/// </summary>
public class GetPrimaryUsersRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Response with list of primary users
/// </summary>
public class PrimaryUsersResponse
{
    public List<string> PrimaryUsers { get; set; } = new();
}

/// <summary>
/// Request to change primary users
/// </summary>
public class ChangePrimaryUsersRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "User list is required")]
    [MinLength(1, ErrorMessage = "At least one user is required")]
    public string[] UserList { get; set; } = Array.Empty<string>();
}
