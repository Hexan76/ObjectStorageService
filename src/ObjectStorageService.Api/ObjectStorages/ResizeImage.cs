using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class ResizeImage : BaseEndpoint<ResizeImageRequest, ProcessImageResult>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("api/{version}/image-processing/resize");
        
        Version(1);
        Tags([ObjectStorageServiceApiTags.ImageProcessing]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ImageProcessing]));
        AllowAnonymous();
        Summary(s => s.Summary = "Resize an image loaded from a URL into the custom sizes sent in the request.");
    }
}
