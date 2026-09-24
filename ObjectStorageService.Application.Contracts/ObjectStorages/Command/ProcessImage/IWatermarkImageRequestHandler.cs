using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface IWatermarkImageRequestHandler : IFrameworkRequestHandler<WatermarkImageRequest, ProcessImageResult>
{
}
