using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using ObjectStorageService.Domain;
using System.Text.RegularExpressions;
using Volo.Abp.Caching;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesHandler(IOptions<ObjectStorageOptions> options, IDistributedCache<ObjectTempCache> distributedCache)
    : IUploadTempFilesRequestHandler
{
    private readonly string _destinationBucket = options.Value.BucketDestination;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;
    private readonly TimeSpan _expirationTime = options.Value.ExpirationTime;
    private readonly IMinioClient _client = new MinioClientFactory(cfg =>
    {
        cfg.WithEndpoint(options.Value.URLDestination)
           .WithCredentials(options.Value.DestinationKey, options.Value.DestinationSecret)
           .WithSSL(options.Value.DestinationSSL)
           .Build();
    }).CreateClient();
    public async Task<MessageContract<UploadTempFilesResponse>> Handle(
        UploadTempFilesRequest request,
        CancellationToken cancellationToken)
    {

        var result = new UploadTempFilesResponse
        {
            Files = []
        };

        foreach (IFormFile file in request.Files)
        {
            var id = Guid.NewGuid();

            var ext = Path.GetExtension(file.FileName);
            var objectName = $"temp/{id}{ext}";

            await using var stream = file.OpenReadStream();

            PutObjectResponse response = null;
            try
            {
                var tags = new Dictionary<string, string>()
                {
                    {"filename",NormalizeTag(file.FileName)},
                    {"storageEntityType",request.StorageEntityType.ToString()},
                    {"storageAppType",request.StorageAppType.ToString()}
                };
                response = await _client.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_destinationBucket)
                        .WithObject(objectName)
                        .WithTagging(new Minio.DataModel.Tags.Tagging(tags, true))
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType),
                    cancellationToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            result.Files.Add(new TempFileModel
            {
                Id = id,
                Name = file.FileName,

                MimeType = file.ContentType,
                Size = (int)file.Length,

                Link =
                    $"{_baseUrl.TrimEnd('/')}/" +
                    $"{_destinationBucket}/" +
                    $"{response.ObjectName}",
            });
            await distributedCache.SetAsync(id.ToString(), new ObjectTempCache()
            {
                Id = id,
                ObjectKey = response?.ObjectName,
                ContentType = file.ContentType,
                CreationTime = DateTime.Now,
                FileName = file.FileName,
                Size = (int)file.Length,
            }, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });
        }


        return new ResultApi<UploadTempFilesResponse>
        {
            Result = result,
            Success = true
        };
    }

    private static string NormalizeTag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        value = Path.GetFileNameWithoutExtension(value);

        value = Regex.Replace(
            value,
            @"[^a-zA-Z0-9_\-\.]",
            "_");

        return value[..Math.Min(value.Length, 128)];
    }
}