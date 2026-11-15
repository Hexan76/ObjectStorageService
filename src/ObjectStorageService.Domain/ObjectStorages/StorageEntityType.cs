using System.Text.Json.Serialization;

namespace ObjectStorageService.ObjectStorages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StorageEntityType
{
    Product = 0,
    Brand = 1,
    Category = 2,
    Review = 3,
    Ticket = 4,
    Profile = 5
}