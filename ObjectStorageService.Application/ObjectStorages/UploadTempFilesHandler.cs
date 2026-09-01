using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MimeDetective;
using MimeDetective.Definitions;
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

    private static readonly IContentInspector Inspector =
    new ContentInspectorBuilder
    {
        Definitions = DefaultDefinitions
        .All()
        .ToList()

    }.Build();
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

            var id = Guid.NewGuid();

            var ext = Path.GetExtension(file.FileName);
            var objectName = $"temp/{id}{ext}";

            await using var stream = file.OpenReadStream();


            var results = Inspector.Inspect(stream);

            var detected = results.FirstOrDefault();


            if (detected is null)
            {
                result.Files.Add(new TempFileModel
                {
                    Id = id,
                    Name = $"Unable to determine file type for '{file.FileName}'.",
                    MimeType = file.ContentType,
                    Size = (int)file.Length,
                    Link = null
                });

                continue;
            }
            var detectedMimeType = detected.Definition.File.MimeType;
            var detectedExtensions = detected.Definition.File.Extensions;

            var requestedExtension = Path
                .GetExtension(file.FileName)
                .TrimStart('.')
                .ToLowerInvariant();

            if (!detectedExtensions.Any(x =>
                    string.Equals(x, requestedExtension, StringComparison.OrdinalIgnoreCase)) && file.ContentType!= "image/svg+xml")
            {
                result.Files.Add(new TempFileModel
                {
                    Id = id,
                    Name = $"File content does not match extension '.{requestedExtension}'.",
                    MimeType = file.ContentType,
                    Size = (int)file.Length,
                    Link = null
                });

                continue;
            }
            await using var streamChecked = file.OpenReadStream();

            PutObjectResponse response = null;

            try
            {
                var tags = new Dictionary<string, string>()
                {
                    {"filename",NormalizeTag(file.FileName)},
                    {"storageEntityType",request.StorageEntityType},
                    {"storageAppType",request.StorageAppType.ToString()}
                };
                response = await _client.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_destinationBucket)
                        .WithObject(objectName)
                        .WithTagging(new Minio.DataModel.Tags.Tagging(tags, true))
                        .WithStreamData(streamChecked)
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
                App = request.StorageAppType
            }, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });
        }


        return MessageContract<UploadTempFilesResponse>.Success(result);

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