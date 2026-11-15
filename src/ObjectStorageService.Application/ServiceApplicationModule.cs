using Framework.BuildingBlock.Application;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using ObjectStorageService.ObjectStorages.Services;
using ObjectStorageService.Application.Contracts;
using ObjectStorageService.Domain;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace ObjectStorageService.Application;

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
        context.Services.AddSingleton<IMinioClientFactory, MinioClientFactory>();
        context.Services.AddScoped<IImageProcessingHelper, ImageProcessingHelper>();
        context.Services.AddScoped<IMinioProcessingHelper, MinioProcessingHelper>();
    }
}
