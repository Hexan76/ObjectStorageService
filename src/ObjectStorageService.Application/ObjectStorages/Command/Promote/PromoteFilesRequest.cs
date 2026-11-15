using Framework.BuildingBlock.Application.Contracts;

namespace ObjectStorageService.ObjectStorages;

public class PromoteFilesRequest : IFrameworkRequest<PromoteFilesResponse>
{
    public bool IsTransaction { get; set; }
    public List<PromoteFileItemRequest> Files { get; set; } = [];

}