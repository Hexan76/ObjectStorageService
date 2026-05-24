namespace ObjectStorageService;

public class ObjectStorageOptions
{
    public string URLSource { get; set; }
    public bool SourceSSL { get; set; } = true;
    public string SourceKey { get; set; }
    public string SourceSecret { get; set; }
    public string BucketSource { get; set; }
    public string URLDestination { get; set; }

    public bool DestinationSSL { get; set; } = true;
    public string DestinationKey { get; set; }
    public string DestinationSecret { get; set; }
    public string BucketDestination { get; set; }

    public string SvgFileFullName { get; set; }

    public string PublicBaseUrl { get; set; } = default!;
    public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromDays(1);

}
