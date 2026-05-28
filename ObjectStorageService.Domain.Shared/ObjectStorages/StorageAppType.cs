using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ObjectStorageService.ObjectStorages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StorageAppType
{
    [Description("4Sough")]
    FourSoughShop= 0,
    QasedFood = 1,
    
}