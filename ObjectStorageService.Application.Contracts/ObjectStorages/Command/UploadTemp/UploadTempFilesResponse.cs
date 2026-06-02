namespace ObjectStorageService.ObjectStorages;

public class UploadTempFilesResponse
{
    public List<TempFileModel> Files { get; set; } = new();
}