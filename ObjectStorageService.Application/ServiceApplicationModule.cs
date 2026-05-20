using Framework.BuildingBlock.Application;
using Microsoft.Extensions.DependencyInjection;
using Service.Template.Application.Contracts;
using Service.Template.Domain;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Service.Template.Application;

[DependsOn(
    typeof(AbpLuckyPennyAutoMapperModule),
    typeof(BuildingBlockApplicationModule),
    typeof(ObjectStorageServiceDomainModule),
    typeof(ObjectStorageServiceApplicationContractsModule)
    )]
public class ObjectStorageServiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ObjectStorageServiceApplicationModule>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ObjectStorageServiceApplicationModule>(validate: true);
        });

        context.Services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ObjectStorageServiceApplicationModule).Assembly);
        });

    }
}
