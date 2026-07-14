namespace ObjectStorageService.ObjectStorages;

public class PromoteItemResponse
{
    public string FileName { get; set; } = null!;
    public string URL { get; set; } = null!;
    public string StorageKey { get; set; } = null!;

    public long Size { get; set; }

    public string MimeType { get; set; } = null!;
    public IDictionary<string, string> Variants { get; set; }

}
