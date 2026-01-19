using AutoMapper;
using Hms.Domains;
using Hms.Domains.Dashboard;
using Hms.Repository;
using Hms.Service.Request.Dashboard;
using Hms.Service.Response;
using Microsoft.Extensions.Configuration;

namespace Hms.Service
{
    public interface IDashboardService
    {
        Task<GenericResponse> AddDashboardDetails(DashboardRequest dashboardRequest, int userId, int tenantId);
    }
    public class DashboardService : BaseService, IDashboardService
    {

        #region Properties
        private readonly IDashboardRepository _dashboardRepository;
        #endregion

        #region Constructor
        public DashboardService(IMapper mapper,
            IConfiguration configuration, IDashboardRepository dashboardRepository) : base(mapper, configuration)
        {
            _dashboardRepository = dashboardRepository;
        }
        #endregion

        #region  AddDashboardDetails
        public async Task<GenericResponse> AddDashboardDetails(DashboardRequest dashboardRequest, int userId, int tenantId)
        {
            var request = _mapper.Map<DashboardRequest, Dashboard>(dashboardRequest);
            var response = await _dashboardRepository.AddDashboardDetails(request, userId, tenantId);
            return _mapper.Map<GenericResult, GenericResponse>(response);
        }
        #endregion
    }
}
