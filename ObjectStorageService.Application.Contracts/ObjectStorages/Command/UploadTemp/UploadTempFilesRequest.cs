using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesRequest : IFrameworkRequest<UploadTempFilesResponse>
{
    public StorageEntityType StorageEntityType { get; set; }
    public StorageAppType StorageAppType { get; set; }
    public List<IFormFile> Files { get; set; } = new();
}