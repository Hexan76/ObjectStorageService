using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using Service.Template.Constants;
using Service.Template.Location;

namespace Service.Template.Locations;

public class Get : BaseEndpoint<GetLocationRequest, LocationModel>
{
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ObjectStorageServiceApiRoutes.LocationRoutes.Default);
        Tags([ObjectStorageServiceApiTags.Locations]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.Locations]));
        //Policies();
        //Permissions();
        AllowAnonymous();
    }
}
