using ObjectStorageService.ObjectStorages;

namespace ObjectStorageService.Constants;

public class ObjectStorageServiceApiRoutes
{
    public const string Prefix = "api";
    public const string Application = $"{Prefix}/ObjectStorageService";

    public static ObjectStoragesRoutes ObjectStorages = new(Application, ObjectStorageServiceApiTags.ObjectStorage);
    public static ImageProcessingRoutes ImageProcessing = new(Application, ObjectStorageServiceApiTags.ImageProcessing);
}
