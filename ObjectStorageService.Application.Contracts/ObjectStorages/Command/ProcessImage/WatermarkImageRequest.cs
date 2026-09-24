using Framework.BuildingBlock.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace ObjectStorageService.ObjectStorages;

public class WatermarkImageRequest : ImageUrlProcessRequest, IFrameworkRequest<ProcessImageResult>
{
    public IFormFile? WatermarkSvg { get; set; }
}
