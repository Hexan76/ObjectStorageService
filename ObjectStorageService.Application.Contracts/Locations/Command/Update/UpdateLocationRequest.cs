using Framework.BuildingBlock.Application.Contracts;
using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class UpdateLocationRequest : EntityDto<Guid>, IFrameworkRequest<BaseResponse>
{
    public string Title { get; set; }
    public string Code { get; set; }
    public Guid ParentId { get; set; }
}
