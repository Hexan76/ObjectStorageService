using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Location;
using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class PaginatedLocationResponse : PagedResultDto<LocationModel>, IFrameworkRequest<BaseResponse>
{

}
