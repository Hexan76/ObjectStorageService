using Volo.Abp.Caching;

namespace ObjectStorageService.Domain;

[CacheName("ObjectTempCache")]
public class ObjectTempCache
{
    public Guid Id { get; set; }

    public string ObjectKey { get; set; } = null!;
    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public DateTime CreationTime { get; set; }
}