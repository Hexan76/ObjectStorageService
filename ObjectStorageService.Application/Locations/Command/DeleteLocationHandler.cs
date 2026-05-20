using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Application;

namespace Service.Template.Locations;

public class DeleteLocationHandler(ILocationRepository locationRepository) : ObjectStorageServiceAppService, IDeleteLocationHandler
{
    public async Task<MessageContract<BaseResponse>> Handle(DeleteLocationRequest request, CancellationToken cancellationToken)
    {
        await locationRepository.DeleteAsync(request.Id);

        return new AcceptMessage<BaseResponse>()
        {
            Data = new BaseResponse() { Id = request.Id },
            Message = L["Location.Deleted"]
        };
    }
}
