using AutoMapper;
using Service.Template.Location;

namespace Service.Template.Locations;

public class LocationProfile : Profile
{
    public LocationProfile()
    {
        CreateMap<CreateLocationRequest, Location>(memberList: MemberList.None)
            ;

        CreateMap<Location, LocationModel>(memberList: MemberList.None)
            ;
        CreateMap<UpdateLocationRequest, Location>(memberList: MemberList.None)
            ;

    }
}
