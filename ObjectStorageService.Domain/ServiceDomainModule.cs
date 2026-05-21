using Framework.BuildingBlock.Domain;
using ObjectStorageService.Domain.Shared;
using Volo.Abp.Modularity;

namespace ObjectStorageService.Domain;

[DependsOn(
    typeof(BuildingBlockDomainModule),
    typeof(ObjectStorageServiceDomainSharedModule)
)]
public class ObjectStorageServiceDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
