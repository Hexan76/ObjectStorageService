namespace ObjectStorageService.ObjectStorages;

public class TempFileModel
{
    public Guid Id { get; set; }
    public string Link { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public string Name { get; set; }
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
}
