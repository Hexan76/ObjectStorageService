namespace ObjectStorageService.ObjectStorages;

public class ProcessImageResult
{
    public string FileName { get; set; } = null!;
    public string SourceUrl { get; set; } = null!;
    public string URL { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }
    public IDictionary<string, string> Variants { get; set; } = new Dictionary<string, string>();
}
