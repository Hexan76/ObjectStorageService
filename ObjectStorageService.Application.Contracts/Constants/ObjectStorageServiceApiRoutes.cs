using Service.Template.Location;

namespace Service.Template.Constants;

public class ObjectStorageServiceApiRoutes
{
    public const string Prefix = "api";
    public const string Application = $"{Prefix}/ObjectStorageService";

    public static LocationRoutes LocationRoutes = new(Application, ObjectStorageServiceApiTags.Locations);
}
