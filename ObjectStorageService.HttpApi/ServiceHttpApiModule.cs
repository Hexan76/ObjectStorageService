using Framework.BuildingBlock.HttpApi;
using Microsoft.Extensions.DependencyInjection;
using Service.Template.Application.Contracts;
using Volo.Abp.Modularity;

namespace Service.Template.HttpApi;

[DependsOn(
    typeof(ObjectStorageServiceApplicationContractsModule),
    typeof(BuildingBlockHttpApiModule))]
public class ObjectStorageServiceHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ObjectStorageServiceHttpApiModule).Assembly);
        });
    }

}
