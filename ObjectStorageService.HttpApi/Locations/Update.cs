using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using Service.Template.Constants;

namespace Service.Template.Locations;

public class Update : BaseEndpoint<UpdateLocationRequest,BaseResponse>
{
    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes(ObjectStorageServiceApiRoutes.LocationRoutes.Update);
        Tags([ObjectStorageServiceApiTags.Locations]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.Locations]));
        //Policies();
        //Permissions();
        AllowAnonymous();
    }
}
