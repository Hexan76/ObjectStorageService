using Framework.BuildingBlock.Domain.Shared;
using Framework.Localization;
using Template.Service.Domain.Shared;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Service.Template.Domain.Shared;

[DependsOn(
    typeof(BuildingBlockDomainSharedModule)
    )]
public class ObjectStorageServiceDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ObjectStorageServiceDomainSharedModule>();
        });

        Configure<LocalizationResourceOptions>(options =>
        {
            options.AddResource<ObjectStorageServiceResource>();
        });

    }
}
