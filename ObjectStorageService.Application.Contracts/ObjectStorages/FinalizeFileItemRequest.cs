namespace ObjectStorageService.ObjectStorages;

public class FinalizeFileItemRequest
{
    public Guid Id { get; set; }

    public string StorageEntityType { get; set; }

    public string EntityKey { get; set; } = null!;

    public StorageAppType AppType { get; set; } 

    public string? FileName { get; set; }

    public bool GenerateThumbnail { get; set; }

    public bool Watermark { get; set; }

    public List<ImageSize> Sizes { get; set; } = [];
}
