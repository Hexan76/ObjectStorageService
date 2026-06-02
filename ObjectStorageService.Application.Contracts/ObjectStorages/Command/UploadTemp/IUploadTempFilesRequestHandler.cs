using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IUploadTempFilesRequestHandler : IFrameworkRequestHandler<UploadTempFilesRequest, UploadTempFilesResponse>
{
}