using Volo.Abp.Reflection;

namespace ObjectStorageService.Permissions;

public class ObjectStorageServicePermissions
{
    public const string GroupName = "Service";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ObjectStorageServicePermissions));
    }
}
