using Framework.BuildingBlock.Application.Contracts;

namespace Service.Template.Locations;

public class CreateLocationRequest : IFrameworkRequest<BaseResponse>
{
    public string Title { get; set; }
    public string Code { get; set; }
    public Guid? ParentId { get; set; }
}
