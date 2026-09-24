using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IThumbnailImageRequestHandler : IFrameworkRequestHandler<ThumbnailImageRequest, ProcessImageResult>
{
}
