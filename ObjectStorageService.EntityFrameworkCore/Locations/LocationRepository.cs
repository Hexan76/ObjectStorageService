using Framework.BuildingBlock.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Service.Template.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Service.Template.Locations;

public class LocationRepository : EfCoreRepository<ObjectStorageServiceDbContext, Location, Guid>, ILocationRepository
{
    public LocationRepository(IDbContextProvider<ObjectStorageServiceDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<(IEnumerable<Location> Items, int Total)> PaginatedFilterAsync(FilterGroup filterGroup, int skip = 0, int totalCount = 10, string sort = "")
    {
        var query = await GetQueryableAsync();
        query = query
            .ApplyFilter(filterGroup)
            .ApplySort(sort);
        var total = await query.CountAsync();
        query = query
            .Skip(skip).Take(totalCount)
            ;

        return (query.ToList(), total);
    }
}
