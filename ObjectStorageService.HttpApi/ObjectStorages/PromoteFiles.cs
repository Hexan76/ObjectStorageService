using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class PromoteFiles : BaseEndpoint<PromoteFilesRequest, PromoteFilesResponse> 
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Version(1);
        Routes(ObjectStorageServiceApiRoutes.ObjectStorages.Promote);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowAnonymous();
    }
}
