using Framework.BuildingBlock.Application;
using Framework.BuildingBlock.Application.Contracts;
using Service.Template.Application;

namespace Service.Template.Locations;

public class PaginatedLocationHandler(ILocationRepository locationRepository) : ObjectStorageServiceAppService, IPaginatedLocationHandler
{
    public async Task<MessageContract<PaginatedLocationResponse>> Handle(PaginatedLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await locationRepository.PaginatedFilterAsync(request.FilterGroup.ToDomain(), request.SkipCount, request.MaxResultCount);

        var response = new PaginatedLocationResponse();

        ObjectMapper.Map(result.Items, response.Items);

        return new AcceptMessage<PaginatedLocationResponse>()
        {
            Data = new PaginatedLocationResponse()
            {
                Items = response.Items,
                TotalCount = result.Total
            }
        };
    }
}
