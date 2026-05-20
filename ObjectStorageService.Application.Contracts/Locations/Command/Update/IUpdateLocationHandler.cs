using Framework.BuildingBlock.Application.Contracts;

namespace Service.Template.Locations;

public interface IUpdateLocationHandler : IFrameworkRequestHandler<UpdateLocationRequest, BaseResponse>
{
    
}
