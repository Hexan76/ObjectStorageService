using ObjectStorageService.Domain;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ObjectStorageService.EntityFrameworkCore;

[ConnectionStringName(ObjectStorageServiceDbProperties.ConnectionStringName)]
public interface IObjectStorageServiceDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
