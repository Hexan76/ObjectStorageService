using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using Service.Template.Constants;

namespace Service.Template.Locations;

public class Delete : BaseEndpoint<DeleteLocationRequest,BaseResponse>
{
    public override void Configure()
    {
        Verbs(Http.DELETE);
        Routes(ObjectStorageServiceApiRoutes.LocationRoutes.Delete);
        Tags([ObjectStorageServiceApiTags.Locations]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.Locations]));
        //Policies();
        //Permissions();
        AllowAnonymous();
    }
}
