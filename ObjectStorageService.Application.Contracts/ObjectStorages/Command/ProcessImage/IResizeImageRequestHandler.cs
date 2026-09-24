using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IResizeImageRequestHandler : IFrameworkRequestHandler<ResizeImageRequest, ProcessImageResult>
{
}
