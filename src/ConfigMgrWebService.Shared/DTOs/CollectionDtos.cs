using System.ComponentModel.DataAnnotations;

namespace ConfigMgrWebService.Shared.DTOs;

/// <summary>
/// Request to add computer to collection
/// </summary>
public class AddComputerToCollectionRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Collection name is required")]
    public string CollectionName { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove computer from collection
/// </summary>
public class RemoveComputerFromCollectionRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Collection name is required")]
    public string CollectionName { get; set; } = string.Empty;
}

/// <summary>
/// Request to add computer to OSD collection
/// </summary>
public class AddComputerToOsdCollectionRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Identifier is required")]
    public string Identifier { get; set; } = string.Empty;
}

/// <summary>
/// Request to remove computer from OSD collection
/// </summary>
public class RemoveComputerFromOsdCollectionRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Advertisement ID is required")]
    public string AdvertisementId { get; set; } = string.Empty;
}

/// <summary>
/// Request to check collection membership
/// </summary>
public class IsMemberOfCollectionRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Collection name is required")]
    public string CollectionName { get; set; } = string.Empty;
}

/// <summary>
/// Response with collection membership status
/// </summary>
public class CollectionMembershipResponse
{
    public bool IsMember { get; set; }
    public string Status { get; set; } = string.Empty; // "MEMBER" or "NOT_MEMBER"
}

/// <summary>
/// Request to trigger restaging of computer
/// </summary>
public class TriggerRestagingRequest
{
    [Required(ErrorMessage = "Computer name is required")]
    public string ComputerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Identifier is required")]
    public string Identifier { get; set; } = string.Empty;
}
