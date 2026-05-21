using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesHandler(
    IOptions<ObjectStorageOptions> options)
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
                    {"filename",file.FileName}
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

            var expiresAt = DateTime.UtcNow.Add(_expirationTime);

            result.Files.Add(new TempFileModel
            {
                Id = id,
                FileName = file.FileName,

                ObjectKey = response?.ObjectName,

                Url =
                    $"{_baseUrl.TrimEnd('/')}/" +
                    $"{_destinationBucket}/" +
                    $"{response.ObjectName}",

                ExpiresAt = expiresAt,
                RemainingTime = expiresAt - DateTime.UtcNow
            });
        }


        return new AcceptMessage<UploadTempFilesResponse>
        {
            Data = result
        };
    }
}