using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class ResizeImageRequest : ImageUrlProcessRequest, IFrameworkRequest<ProcessImageResult>
{
    public List<ProcessImageSize> Sizes { get; set; } = [];
}
