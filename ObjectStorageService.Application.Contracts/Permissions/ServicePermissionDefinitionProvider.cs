using Template.Service.Domain.Shared;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Service.Template.Permissions;

public class ObjectStorageServicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ObjectStorageServicePermissions.GroupName, L("Permission:Service"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ObjectStorageServiceResource>(name);
    }
}
