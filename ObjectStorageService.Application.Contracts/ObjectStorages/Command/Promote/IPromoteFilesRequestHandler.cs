using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IPromoteFilesRequestHandler : IFrameworkRequestHandler<PromoteFilesRequest, PromoteFilesResponse>
{
}