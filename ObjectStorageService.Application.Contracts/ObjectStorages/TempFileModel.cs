namespace ObjectStorageService.ObjectStorages;

public class TempFileModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public string ObjectKey { get; set; } = default!;
}
