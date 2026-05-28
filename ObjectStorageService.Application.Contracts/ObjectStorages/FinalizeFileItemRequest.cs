namespace ObjectStorageService.ObjectStorages;

public class FinalizeFileItemRequest
{
    public Guid Id { get; set; }

    public StorageEntityType StorageEntityType { get; set; }

    public string EntityKey { get; set; } = null!;

    public string AppName { get; set; } = null!;

    public string? FileName { get; set; }

    public bool GenerateThumbnail { get; set; }

    public bool Watermark { get; set; }

    public List<ImageSize> Sizes { get; set; } = [];
}
