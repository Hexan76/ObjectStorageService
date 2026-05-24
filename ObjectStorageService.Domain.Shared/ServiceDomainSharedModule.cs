using Framework.BuildingBlock.Domain.Shared;
using Framework.Localization;
using Microsoft.Extensions.Configuration;
using Template.Service.Domain.Shared;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace ObjectStorageService.Domain.Shared;

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

        var objectStorageSection = context.Configuration.GetSection("ObjectStorage");
        Configure<ObjectStorageOptions>(options =>
        {
            options.URLSource = objectStorageSection["URLSource"] ?? "sourceURL";
            options.SourceSSL = objectStorageSection.GetValue<bool>("SourceSSL");
            options.SourceKey = objectStorageSection["SourceKey"] ?? "SourceKey";
            options.SourceSecret = objectStorageSection["SourceSecret"] ?? "SourceSecret";
            options.BucketSource = objectStorageSection["BucketSource"] ?? "4sough-Stage";

            options.URLDestination = objectStorageSection["URLDestination"] ?? "destinationURL";
            options.DestinationSSL = objectStorageSection.GetValue<bool>("DestinationSSL");
            options.DestinationKey = objectStorageSection["DestinationKey"] ?? "DestinationKey";
            options.DestinationSecret = objectStorageSection["DestinationSecret"] ?? "DestinationSecret";
            options.BucketDestination = objectStorageSection["BucketDestination"] ?? "4sough-Stage";

            options.SvgFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "svg.svg");
            options.PublicBaseUrl = objectStorageSection["PublicBaseUrl"] ?? "https://localhost:7298";
        });

    }
}
