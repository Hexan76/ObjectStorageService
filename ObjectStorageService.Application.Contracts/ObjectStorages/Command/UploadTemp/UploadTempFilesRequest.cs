using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesRequest : IFrameworkRequest<UploadTempFilesResponse>
{
    public List<IFormFile> Files { get; set; } = new();
}