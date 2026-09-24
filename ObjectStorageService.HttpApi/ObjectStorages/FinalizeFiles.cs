using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class FinalizeFiles : BaseEndpoint<FinalizeFilesRequest, FinalizeFilesResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        //Routes(ObjectStorageServiceApiRoutes.ObjectStorages.Finalize);
        Routes("{version}/api/ObjectStorageService/object-storage/finalize", "api/{version}/object-storage/finalize");

        Version(1);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowAnonymous();
    }

    public override async Task<MessageContract<FinalizeFilesResponse>> ExecuteAsync(FinalizeFilesRequest req, CancellationToken ct)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status207MultiStatus;
        return await base.ExecuteAsync(req, ct);
    }
}
