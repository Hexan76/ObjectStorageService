using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Domain.Shared;
using Volo.Abp.Modularity;

namespace Service.Template.Application.Contracts;

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
