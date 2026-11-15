namespace ObjectStorageService.ObjectStorages;

public class ImageUrlProcessRequest
{
    public string ImageUrl { get; set; } = null!;
    /// <summary>
    /// When true, the generated file is stored in the same storage directory as <see cref="ImageUrl"/>.
    /// </summary>
    public bool IsCurrentDirectory { get; set; }
    public string StorageEntityType { get; set; } = null!;
    public StorageAppType StorageAppType { get; set; }
    public string EntityKey { get; set; } = null!;

}
