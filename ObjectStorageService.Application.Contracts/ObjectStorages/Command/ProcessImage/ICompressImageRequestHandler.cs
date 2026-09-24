using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public interface ICompressImageRequestHandler : IFrameworkRequestHandler<CompressImageRequest, ProcessImageResult>
{
}
