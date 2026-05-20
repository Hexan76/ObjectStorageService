using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Location;

namespace Service.Template.Locations;

public interface IGetLocationHandler : IFrameworkRequestHandler<GetLocationRequest, LocationModel>
{
    
}
