using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class FinalizeFiles : BaseEndpoint<FinalizeFilesRequest, FinalizeFilesResponse> 
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Version(1);
        Routes(ObjectStorageServiceApiRoutes.ObjectStorages.Finalize);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowAnonymous();
    }
}
