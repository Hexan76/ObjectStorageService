using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IGetTempFilesRequestHandler : IFrameworkRequestHandler<GetTempFilesRequest, UploadTempFilesResponse>
{
}