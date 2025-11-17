using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;

namespace ConfigMgrWebService.Infrastructure.Graph.Models;

/// <summary>
/// Response model for ManagedDevice collection
/// </summary>
public class ManagedDeviceCollectionResponse
{
    [JsonProperty("value")]
    public List<ManagedDevice> Value { get; set; } = new();
}

/// <summary>
/// Response model for User collection
/// </summary>
public class UserCollectionResponse
{
    [JsonProperty("value")]
    public List<User> Value { get; set; } = new();
}

/// <summary>
/// Response model for Device collection
/// </summary>
public class DeviceCollectionResponse
{
    [JsonProperty("value")]
    public List<Device> Value { get; set; } = new();
}

/// <summary>
/// Response model for Group collection
/// </summary>
public class GroupCollectionResponse
{
    [JsonProperty("value")]
    public List<Microsoft.Graph.Beta.Models.Group> Value { get; set; } = new();
}

/// <summary>
/// Response model for DirectoryObject collection
/// </summary>
public class DirectoryObjectCollectionResponse
{
    [JsonProperty("value")]
    public List<DirectoryObject> Value { get; set; } = new();
}

/// <summary>
/// Response model for DeviceCategory collection
/// </summary>
public class DeviceCategoryCollectionResponse
{
    [JsonProperty("value")]
    public List<DeviceCategory> Value { get; set; } = new();
}
