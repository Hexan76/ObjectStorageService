using Framework.BuildingBlock.Application.Contracts;

namespace Service.Template.Locations;

public interface IDeleteLocationHandler : IFrameworkRequestHandler<DeleteLocationRequest, BaseResponse>
{
    
}
