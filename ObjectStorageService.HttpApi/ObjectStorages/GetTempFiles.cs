using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class GetTempFiles : BaseEndpoint<GetTempFilesRequest, UploadTempFilesResponse> 
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Version(1);
        Routes(ObjectStorageServiceApiRoutes.ObjectStorages.GetTempFiles);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowFileUploads();
        AllowAnonymous();
    }
}
