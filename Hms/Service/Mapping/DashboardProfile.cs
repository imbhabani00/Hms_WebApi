using AutoMapper;
using Hms.Domains;
using Hms.Domains.Dashboard;
using Hms.Service.Request.Dashboard;
using Hms.Service.Response;

namespace Hms.Service.Mapping
{
    public class DashboardProfile : Profile
    {
        public DashboardProfile()
        {
            CreateMap<GenericResult, GenericResponse>();
            CreateMap<DashboardRequest, Dashboard>();
        }
    }
}
