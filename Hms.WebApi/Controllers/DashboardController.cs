using Hms.Infrastructure.Cache;
using Hms.Service;
using Hms.Service.Request.Dashboard;
using Hms.Service.Response;
using Hms.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hms.WebApi.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class DashboardController : BaseApiController
    {
        #region Properties
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public DashboardController(IDashboardService dashboardService,
            ICachedConfigurationService cachedConfigurationService,
            IConfiguration configuration,
            ILogger<DashboardController> logger) : base(configuration, cachedConfigurationService)
        {
            _configuration = configuration;
            _dashboardService = dashboardService;
            _logger = logger;
        }
        #endregion

        #region AddDashboardDetails
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> AddDashboardDetails(DashboardRequest dashboardRequest)
        {
            var apiResponse = new ApiResponse();
            try
            {
                ResolveUserIdentity();
                int tenantId = Convert.ToInt32(this.User.Identity.GetTenantId());
                int loggedInUserId = Convert.ToInt32(this.User.Identity.GetUserId());

                var response = await _dashboardService.AddDashboardDetails(dashboardRequest, loggedInUserId, tenantId);
                switch (response.ReturnValue)
                {
                    case 0:
                        apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "Dashboard details saved successfully.");
                        break;

                    case 1:
                        apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "Dashboard details updated succesfully.");
                        break;

                    default:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Internal server error.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in add dashboard details");
                apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Internal server error");
            }
            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}
