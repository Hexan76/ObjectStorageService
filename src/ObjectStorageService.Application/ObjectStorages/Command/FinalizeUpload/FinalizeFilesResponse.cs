namespace ObjectStorageService.ObjectStorages;

public class FinalizeFilesResponse
{
    public List<FinalizeItemResponse> Files { get; set; } = new();
}