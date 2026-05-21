using Framework.BuildingBlock.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObjectStorageService.Domain;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace ObjectStorageService.EntityFrameworkCore;

[DependsOn(
    typeof(ObjectStorageServiceDomainModule),
    typeof(BuildingBlockEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
public class ObjectStorageServiceEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ObjectStorageServiceDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);

            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
        
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });
    }
}
