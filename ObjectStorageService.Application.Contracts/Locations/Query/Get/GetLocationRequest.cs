using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Location;
using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class GetLocationRequest : EntityDto<Guid>, IFrameworkRequest<LocationModel>
{

}
