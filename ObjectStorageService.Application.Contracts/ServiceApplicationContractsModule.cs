using Framework.BuildingBlock.Application.Contracts;
using ObjectStorageService.Domain.Shared;
using Volo.Abp.Modularity;

namespace ObjectStorageService.Application.Contracts;

[DependsOn(
    typeof(ObjectStorageServiceDomainSharedModule),
    typeof(BuildingBlockApplicationContractsModule)
    )]
public class ObjectStorageServiceApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
