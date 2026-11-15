using Framework.BuildingBlock.HttpApi;
using Microsoft.AspNetCore.Http;
using ObjectStorageService.Constants;

namespace ObjectStorageService.ObjectStorages;

public class GetTempFiles : BaseEndpoint<GetTempFilesRequest, UploadTempFilesResponse>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("{version}/api/ObjectStorageService/object-storage/get-files", "api/{version}/object-storage/get-files");

        Version(1);
        Tags([ObjectStorageServiceApiTags.ObjectStorage]);
        Options(c => c.WithTags([ObjectStorageServiceApiTags.ObjectStorage]));
        //Policies();
        //Permissions();

        AllowFileUploads();
        AllowAnonymous();
    }
}
