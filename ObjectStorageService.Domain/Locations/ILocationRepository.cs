using Framework.BuildingBlock.Domain.Shared;
using Volo.Abp.Domain.Repositories;

namespace Service.Template.Locations;

public interface ILocationRepository : IRepository<Location, Guid>
{
    Task<(IEnumerable<Location> Items, int Total)> PaginatedFilterAsync(FilterGroup filterGroup, int skip = 0, int totalCount = 10, string sort = "");
    //cusomize method
}
