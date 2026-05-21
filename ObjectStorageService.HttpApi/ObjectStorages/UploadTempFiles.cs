using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFiles : BaseEndpoint<UploadTempFilesRequest, UploadTempFilesResponse> 
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Version(1);
        Routes(ObjectStorageServiceApiRoutes.ObjectStorages.Temp);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowFileUploads();
        AllowAnonymous();
    }
}
