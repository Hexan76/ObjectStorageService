using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class FinalizeFilesRequest : IFrameworkRequest<FinalizeFilesResponse>
{
    public bool IsTransaction { get; set; }
    public List<FinalizeFileItemRequest> Files { get; set; } = [];
}