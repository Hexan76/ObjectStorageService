using Volo.Abp.Application.Dtos;

namespace Service.Template.Locations;

public class LocationModel : EntityDto<Guid>
{
    public string Title { get; set; }
    public string Code { get; set; }
    public LocationModel Parent { get; set; }
}
