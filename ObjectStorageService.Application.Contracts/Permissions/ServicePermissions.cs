using Volo.Abp.Reflection;

namespace Service.Template.Permissions;

public class ObjectStorageServicePermissions
{
    public const string GroupName = "Service";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ObjectStorageServicePermissions));
    }
}
