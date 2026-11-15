using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class WatermarkImage : BaseEndpoint<WatermarkImageRequest, ProcessImageResult>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("api/{version}/image-processing/watermark");

        Version(1);
        Tags([ObjectStorageServiceApiTags.ImageProcessing]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ImageProcessing]));
        AllowFileUploads();
        AllowAnonymous();
        Summary(s => s.Summary = "Apply an uploaded SVG watermark to the original image loaded from a URL.");
    }
}
