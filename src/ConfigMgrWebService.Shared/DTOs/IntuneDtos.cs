using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to check if device exists in Intune
/// </summary>
public class CheckIntuneDeviceExistsRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Response with Intune device existence status
/// </summary>
public class IntuneDeviceExistsResponse
{
    public bool Exists { get; set; }
    public string Status { get; set; } = string.Empty; // "EXISTS" or "NOT_FOUND"
}

/// <summary>
/// Request to set primary user in Intune
/// </summary>
public class SetPrimaryUserIntuneRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "User name is required")]
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// Request to set device category in Intune
/// </summary>
public class SetDeviceCategoryIntuneRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Device category name is required")]
    public string DeviceCategoryName { get; set; } = string.Empty;
}

/// <summary>
/// Request to check if device is co-managed
/// </summary>
public class CheckCoManagedRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;
}

/// <summary>
/// Response with co-management status
/// </summary>
public class CoManagedStatusResponse
{
    public bool IsCoManaged { get; set; }
    public string Status { get; set; } = string.Empty; // "CO_MANAGED" or "NOT_CO_MANAGED"
}
