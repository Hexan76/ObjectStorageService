using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class ThumbnailImage : BaseEndpoint<ThumbnailImageRequest, ProcessImageResult>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("api/{version}/image-processing/thumbnail");
        
        Version(1);
        Tags([ObjectStorageServiceApiTags.ImageProcessing]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ImageProcessing]));
        AllowAnonymous();
        Summary(s => s.Summary = "Create a thumbnail from the original image loaded from a URL.");
    }
}
