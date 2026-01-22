using AutoMapper;
using Hms.Domains.UserProfile;
using Hms.Service.Response.UserProfile;

namespace Hms.Service.Mapping
{
    public class UserProfileProfile : Profile
    {
        public UserProfileProfile()
        {
            CreateMap<Hms.Domains.UserProfile.UserProfile, UserProfileResponse>();
            CreateMap<UserProfileGet, UserProfileResponseGet>();
        }
    }
}
