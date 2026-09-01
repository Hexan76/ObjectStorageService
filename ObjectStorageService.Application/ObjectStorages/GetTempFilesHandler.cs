using Framework.BuildingBlock.Application.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using ObjectStorageService.Domain;
using Volo.Abp.Caching;

namespace ObjectStorageService.ObjectStorages;

public sealed class GetTempFilesHandler(
    IDistributedCache<ObjectTempCache> distributedCache,
    Microsoft.Extensions.Options.IOptions<ObjectStorageOptions> options)
    : IGetTempFilesRequestHandler
{
    private readonly string _environment = options.Value.Environment;
    private readonly string _baseUrl = options.Value.PublicBaseUrl;

    public async Task<MessageContract<UploadTempFilesResponse>> Handle(
        GetTempFilesRequest request,
        CancellationToken cancellationToken)
    {
        var result = new UploadTempFilesResponse
        {
            Files = []
        };

        foreach (var fileId in request.Files.Distinct())
        {
            var tempFile = await distributedCache.GetAsync(fileId.ToString());

            if (tempFile is null)
            {
                continue;
            }

            var bucket = StorageAppMapping.GetStorageAppType(
                tempFile.App,
                _environment);

            result.Files.Add(new TempFileModel
            {
                Id = tempFile.Id,
                Name = tempFile.FileName,
                MimeType = tempFile.ContentType,
                Size = tempFile.Size,
                Link =
                    $"{_baseUrl.TrimEnd('/')}/" +
                    $"{bucket}/" +
                    $"{tempFile.ObjectKey}"
            });
        }

        return MessageContract<UploadTempFilesResponse>.Success(result);
    }
}