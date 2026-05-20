using Service.Template.Locations;

namespace Service.Template;

public interface IBaseRepository
{
    public ILocationRepository LocationRepository { get; set; }
}
