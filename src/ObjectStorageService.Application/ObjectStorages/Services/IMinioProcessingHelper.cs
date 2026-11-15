using Minio.DataModel;
using Minio.DataModel.Response;

namespace ObjectStorageService.ObjectStorages.Services;

public interface IMinioProcessingHelper
{
    Task<ObjectStat> StatObjectAsync(string bucket, string objectName, CancellationToken ct);
    Task<Dictionary<string, string>> GetObjectTagsAsync(string bucket, string objectName, CancellationToken ct);
    Task<MemoryStream> GetObjectAsStreamAsync(string bucket, string objectName, CancellationToken ct);
    Task<PutObjectResponse?> PutObjectAsync(string bucket, string objectName, Stream data, string contentType, Dictionary<string, string>? tags = null, CancellationToken ct = default);
    Task CopyObjectAsync(string bucket, string destObject, string sourceObject, CancellationToken ct);
    Task RemoveObjectAsync(string bucket, string objectName, CancellationToken ct);
}
