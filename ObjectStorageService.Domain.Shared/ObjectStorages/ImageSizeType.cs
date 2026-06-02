using System.Text.Json.Serialization;

namespace ObjectStorageService.ObjectStorages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImageSize
{
    Small,
    Medium,
    Large
}