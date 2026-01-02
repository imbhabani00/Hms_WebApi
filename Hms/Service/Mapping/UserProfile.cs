using AutoMapper;
using Hms.Domains;
using Hms.Domains.User;
using Hms.Service.Request.Users;
using Hms.Service.Response;
using Hms.Service.Response.Users;

namespace Hms.Service.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<GenericResult, GenericResponse>();  
            CreateMap<UserRequest, Domains.User.User>();  
            CreateMap<User, UserResponse>();  
            CreateMap<UserGet, UserResponseGet>();  
        }
    }
}
