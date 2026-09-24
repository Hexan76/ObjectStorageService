using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class CompressImageRequest : ImageUrlProcessRequest, IFrameworkRequest<ProcessImageResult>
{
}
