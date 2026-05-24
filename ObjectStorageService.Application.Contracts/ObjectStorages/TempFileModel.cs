namespace ObjectStorageService.ObjectStorages;

public class TempFileModel
{
    public Guid Id { get; set; }
    public string ObjectKey { get; set; } = default!;
    public string Link { get; set; }
    public string MimeType { get; set; }
    public int Size { get; set; }
    public string Name { get; set; }
}
