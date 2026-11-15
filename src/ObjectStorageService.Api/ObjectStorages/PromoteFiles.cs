using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class PromoteFiles : BaseEndpoint<PromoteFilesRequest, PromoteFilesResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        //Routes(ObjectStorageServiceApiRoutes.ObjectStorages.Promote);
        Routes("{version}/api/ObjectStorageService/object-storage/promote", "api/{version}/object-storage/promote");

        Version(1);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowAnonymous();
    }
}
