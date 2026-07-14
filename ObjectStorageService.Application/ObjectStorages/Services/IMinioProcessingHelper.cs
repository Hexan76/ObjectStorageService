using Minio.DataModel;

namespace ObjectStorageService.ObjectStorages.Services;

public interface IMinioProcessingHelper
{
    Task<ObjectStat> StatObjectAsync(string bucket, string objectName, CancellationToken ct);
    Task<Dictionary<string, string>> GetObjectTagsAsync(string bucket, string objectName, CancellationToken ct);
    Task<MemoryStream> GetObjectAsStreamAsync(string bucket, string objectName, CancellationToken ct);
    Task PutObjectAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct);
    Task CopyObjectAsync(string bucket, string destObject, string sourceObject, CancellationToken ct);
    Task RemoveObjectAsync(string bucket, string objectName, CancellationToken ct);
}
