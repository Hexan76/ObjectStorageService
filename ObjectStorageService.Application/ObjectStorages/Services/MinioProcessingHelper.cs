using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Volo.Abp.DependencyInjection;

namespace ObjectStorageService.ObjectStorages.Services;

public class MinioProcessingHelper : IMinioProcessingHelper, IScopedDependency
{
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly bool _useSsl;
    private readonly IMinioClient _client;

    public MinioProcessingHelper(IOptions<ObjectStorageOptions> options)
    {
        _endpoint = options.Value.URLDestination;
        _accessKey = options.Value.DestinationKey;
        _secretKey = options.Value.DestinationSecret;
        _useSsl = options.Value.DestinationSSL;

        _client = new MinioClientFactory(cfg =>
        {
            cfg.WithEndpoint(_endpoint)
               .WithCredentials(_accessKey, _secretKey)
               .WithSSL(_useSsl)
               .Build();
        }).CreateClient();
    }

    public async Task<Dictionary<string, string>> GetObjectTagsAsync(string bucket, string objectName, CancellationToken ct)
    {
        var tagsObj = await _client.GetObjectTagsAsync(new GetObjectTagsArgs().WithBucket(bucket).WithObject(objectName), ct);

        if (tagsObj == null)
            return new Dictionary<string, string>();

        //// If it's already a dictionary
        //if (tagsObj is Dictionary<string, string> dict)
        //    return dict;

        // Try to access a 'Tags' property via reflection
        var prop = tagsObj.GetType().GetProperty("Tags");
        if (prop != null)
        {
            var val = prop.GetValue(tagsObj);
            if (val is Dictionary<string, string> d) return d;
            if (val is IEnumerable<KeyValuePair<string, string>> kv)
                return kv.ToDictionary(x => x.Key, x => x.Value);
        }

        // Fallback: try to enumerate public properties
        var result = new Dictionary<string, string>();
        foreach (var p in tagsObj.GetType().GetProperties())
        {
            try
            {
                var v = p.GetValue(tagsObj)?.ToString();
                if (v != null) result[p.Name] = v;
            }
            catch { }
        }

        return result;
    }

    public async Task<MemoryStream> GetObjectAsStreamAsync(string bucket, string objectName, CancellationToken ct)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithCallbackStream(s => s.CopyTo(ms)),
            ct);
        ms.Position = 0;
        return ms;
    }

    public Task PutObjectAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct)
    {
        data.Position = 0;
        return _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType),
            ct);
    }

    public Task CopyObjectAsync(string bucket, string destObject, string sourceObject, CancellationToken ct)
        => _client.CopyObjectAsync(
            new CopyObjectArgs()
                .WithBucket(bucket)
                .WithObject(destObject)
                .WithCopyObjectSource(new CopySourceObjectArgs().WithBucket(bucket).WithObject(sourceObject)),
            ct);

    public Task RemoveObjectAsync(string bucket, string objectName, CancellationToken ct)
        => _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucket).WithObject(objectName), ct);

    public Task<ObjectStat> StatObjectAsync(string bucket, string objectName, CancellationToken ct)
        => _client.StatObjectAsync(new StatObjectArgs().WithBucket(bucket).WithObject(objectName), ct);
}
