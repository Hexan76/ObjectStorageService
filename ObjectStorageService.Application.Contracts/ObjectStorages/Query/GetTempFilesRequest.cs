using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class GetTempFilesRequest : IFrameworkRequest<UploadTempFilesResponse>
{
    public ICollection<Guid> Files { get; set; } = [];
}