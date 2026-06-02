using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ObjectStorageService.ObjectStorages;

public static class StorageAppMapping
{
    public static string GetStorageAppType(StorageAppType appType, string postFix = "stage")
    {
        return appType switch
        {
            StorageAppType.CharSoughShop => $"4sough-{postFix}",
            StorageAppType.QasedFood => $"qasedfood-{postFix}",
            StorageAppType.QasedParcel => $"qasedparcel-{postFix}",
            _ => throw new ArgumentException($"Unknown app type: {appType}")
        };
    }
}