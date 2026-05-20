using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Application;

namespace Service.Template.Locations;

public class UpdateLocationHandler(ILocationRepository locationRepository) : ObjectStorageServiceAppService, IUpdateLocationHandler
{
    public async Task<MessageContract<BaseResponse>> Handle(UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var founded = await locationRepository.GetAsync(request.Id);

        ObjectMapper.Map(request, founded);

        var result = await locationRepository.UpdateAsync(founded);

        return new AcceptMessage<BaseResponse>()
        {
            Data = new BaseResponse() { Id = result.Id },
            Message = L["Location.Updated"]
        };
    }
}
