using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using ObjectStorageService.Domain;
using System.Text.RegularExpressions;
using Template.Service.Domain.Shared;
using Volo.Abp.Caching;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesHandler(
    IOptions<ObjectStorageOptions> options,
    IDistributedCache<ObjectTempCache> distributedCache,
    IStringLocalizer<ObjectStorageServiceResource> L)
    : IUploadTempFilesRequestHandler
{
    private string _destinationBucket = options.Value.BucketDestination;
    private readonly string _environment = options.Value.Environment;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;
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
            _destinationBucket = StorageAppMapping.GetStorageAppType(request.StorageAppType, _environment);

            var validation = await FileValidator.ValidateAsync(
                file,
                cancellationToken);

            if (!validation.IsValid)
            {
                result.Files.Add(FailedTempFile(file, Localize(validation.Error, validation.ErrorArgs)));
                continue;
            }

            var id = Guid.NewGuid();
            var ext = Path.GetExtension(file.FileName);
            var objectName = $"temp/{id}{ext}";

            try
            {
                await using var streamChecked = file.OpenReadStream();

                var tags = new Dictionary<string, string>
                {
                    { "filename", NormalizeTag(file.FileName) },
                    { "storageEntityType", request.StorageEntityType },
                    { "storageAppType", request.StorageAppType.ToString() }
                };

                var response = await _client.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_destinationBucket)
                        .WithObject(objectName)
                        .WithTagging(new Minio.DataModel.Tags.Tagging(tags, true))
                        .WithStreamData(streamChecked)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType),
                    cancellationToken);

                result.Files.Add(new TempFileModel
                {
                    Id = id,
                    Name = file.FileName,
                    MimeType = file.ContentType,
                    Size = file.Length,
                    Link =
                        $"{_baseUrl.TrimEnd('/')}/" +
                        $"{_destinationBucket}/" +
                        $"{response.ObjectName}",
                    Success = true
                });

                await distributedCache.SetAsync(id.ToString(), new ObjectTempCache
                {
                    Id = id,
                    ObjectKey = response.ObjectName,
                    ContentType = file.ContentType,
                    CreationTime = DateTime.Now,
                    FileName = file.FileName,
                    Size = (int)file.Length,
                    App = request.StorageAppType
                }, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
            }
            catch (Exception)
            {
                result.Files.Add(FailedTempFile(file, L[FileLocalizationKeys.UploadFailed]));
            }
        }

        return MessageContract<UploadTempFilesResponse>.Success(result);
    }

    private string Localize(string? key, object[]? args)
    {
        if (string.IsNullOrWhiteSpace(key))
            return L[FileLocalizationKeys.UploadFailed];

        return args is { Length: > 0 }
            ? L[key, args]
            : L[key];
    }

    private static TempFileModel FailedTempFile(IFormFile file, string? error)
        => new()
        {
            Name = file.FileName,
            MimeType = file.ContentType,
            Size = file.Length,
            Success = false,
            Error = error
        };

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