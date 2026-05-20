using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using Service.Template.Constants;

namespace Service.Template.Locations;

public class Paginated : BaseEndpoint<PaginatedLocationRequest, PaginatedLocationResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ObjectStorageServiceApiRoutes.LocationRoutes.GetPaginated);
        Tags([ObjectStorageServiceApiTags.Locations]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.Locations]));
        //Policies();
        //Permissions();
        AllowAnonymous();
    }
}
