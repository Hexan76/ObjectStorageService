using Framework.BuildingBlock.Application.Contracts;
using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class DeleteLocationRequest : EntityDto<Guid>, IFrameworkRequest<BaseResponse>
{

}
