using AutoMapper;
using Hms.Domains;
using Hms.Service.Request.Users;
using Hms.Service.Response;

namespace Hms.Service.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<GenericResult, GenericResponse>();  
            CreateMap<UserRequest, Domains.User.User>();  
        }
    }
}
