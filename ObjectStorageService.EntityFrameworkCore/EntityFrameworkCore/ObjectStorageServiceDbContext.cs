using Service.Template.Domain;
using Microsoft.EntityFrameworkCore;

using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using System.Reflection;

namespace Service.Template.EntityFrameworkCore;

[ConnectionStringName(ObjectStorageServiceDbProperties.ConnectionStringName)]
public class ObjectStorageServiceDbContext : AbpDbContext<ObjectStorageServiceDbContext>, IObjectStorageServiceDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public ObjectStorageServiceDbContext(DbContextOptions<ObjectStorageServiceDbContext> options)
        : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies()
        ;
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.RegisterEntityConfigurations(Assembly.GetExecutingAssembly());
    }
}
