using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ThumbnailImageRequest : ImageUrlProcessRequest, IFrameworkRequest<ProcessImageResult>
{
}
