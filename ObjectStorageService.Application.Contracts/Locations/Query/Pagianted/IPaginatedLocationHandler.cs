using Framework.BuildingBlock.Application.Contracts;

namespace Service.Template.Locations;

public interface IPaginatedLocationHandler : IFrameworkRequestHandler<PaginatedLocationRequest, PaginatedLocationResponse>
{
    
}
