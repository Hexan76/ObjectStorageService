using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using Service.Template.Constants;

namespace Service.Template.Locations;

public class Create : BaseEndpoint<CreateLocationRequest,BaseResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ObjectStorageServiceApiRoutes.LocationRoutes.Default);
        Tags([ObjectStorageServiceApiTags.Locations]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.Locations]));
        //Policies();
        //Permissions();
        AllowAnonymous();
    }
}
