using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Application;
using Service.Template.Location;

namespace Service.Template.Locations;

public class GetLocationHandler(ILocationRepository locationRepository) : ObjectStorageServiceAppService, IGetLocationHandler
{
    public async Task<MessageContract<LocationModel>> Handle(GetLocationRequest request, CancellationToken cancellationToken)
    {
        var founded = await locationRepository.GetAsync(request.Id);

        var result = ObjectMapper.Map<Location, LocationModel>(founded);

        return new AcceptMessage<LocationModel>()
        {
            Data = result
        };
    }
}
