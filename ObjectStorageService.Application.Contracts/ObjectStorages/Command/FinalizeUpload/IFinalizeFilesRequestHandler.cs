using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IFinalizeFilesRequestHandler : IFrameworkRequestHandler<FinalizeFilesRequest, FinalizeFilesResponse>
{
}