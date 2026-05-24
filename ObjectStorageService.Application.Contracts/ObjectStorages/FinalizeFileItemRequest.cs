namespace ObjectStorageService.ObjectStorages;

public class FinalizeFileItemRequest
{
    public string ObjectKey { get; set; }

    public StorageEntityType StorageEntityType { get; set; }

    public string EntityKey { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public string? FileName { get; set; }

    public bool GenerateThumbnail { get; set; }

    public bool Watermark { get; set; }

    public List<ImageSize> Sizes { get; set; } = [];
}
