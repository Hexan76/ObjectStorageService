namespace ObjectStorageService.ObjectStorages;

public class FinalizeItemResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;
    public string URL { get; set; } = null!;

    public long Size { get; set; }

    public string MimeType { get; set; } = null!;
    public List<FileVariant> Variants { get; set; } = [];

}
