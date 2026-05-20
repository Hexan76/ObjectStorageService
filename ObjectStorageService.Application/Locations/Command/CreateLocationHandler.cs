using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Application;

namespace Service.Template.Locations;

public class CreateLocationHandler(ILocationRepository locationRepository) : ObjectStorageServiceAppService, ICreateLocationHandler
{
    public async Task<MessageContract<BaseResponse>> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var createModel = ObjectMapper.Map<CreateLocationRequest, Location>(request);

        var result = await locationRepository.InsertAsync(createModel);

        return new AcceptMessage<BaseResponse>()
        {
            Data = new BaseResponse() { Id = result.Id }
        };
    }
}
