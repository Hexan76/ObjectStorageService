using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ObjectStorageService.EntityFrameworkCore;

public class ObjectStorageServiceDbContextFactory : IDesignTimeDbContextFactory<ObjectStorageServiceDbContext>
{
    public ObjectStorageServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        ObjectStorageServiceEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<ObjectStorageServiceDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Write"));

        return new ObjectStorageServiceDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ObjectStorageService.Host/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}