using Framework.BuildingBlock.Application.Contracts;
using Framework.BuildingBlock.Contracts;
using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class PaginatedLocationRequest : PagedAndSortedResultRequestDto, IFrameworkRequest<PaginatedLocationResponse>
{
    public FilterGroupDto FilterGroup { get; set; }
}
