using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class CompressImage : BaseEndpoint<CompressImageRequest, ProcessImageResult>
{
    public override void Configure()
    {
        Post("api/{version}/image-processing/compress");
        Version(1);
        Tags([ObjectStorageServiceApiTags.ImageProcessing]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ImageProcessing]));
        AllowAnonymous();

        Summary(s =>
            s.Summary = "Compress the original image loaded from a URL.");
    }
}
