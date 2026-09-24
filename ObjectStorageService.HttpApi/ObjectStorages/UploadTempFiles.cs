using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class UploadTempFiles : BaseEndpoint<UploadTempFilesRequest, UploadTempFilesResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("{version}/api/ObjectStorageService/object-storage/temp", "api/{version}/object-storage/temp");

        Version(1);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowFileUploads();
        AllowAnonymous();
    }

    public override async Task<MessageContract<UploadTempFilesResponse>> ExecuteAsync(UploadTempFilesRequest req, CancellationToken ct)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status207MultiStatus;
        return await base.ExecuteAsync(req, ct);
    }
}
