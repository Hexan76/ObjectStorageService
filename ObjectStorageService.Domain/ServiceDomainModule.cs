using Framework.BuildingBlock.Domain;
using Service.Template.Domain.Shared;
using Volo.Abp.Modularity;

namespace Service.Template.Domain;

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
