using Framework.BuildingBlock.Application.Contracts;

namespace Service.Template.Locations;

public interface ICreateLocationHandler : IFrameworkRequestHandler<CreateLocationRequest, BaseResponse>
{
    
}
