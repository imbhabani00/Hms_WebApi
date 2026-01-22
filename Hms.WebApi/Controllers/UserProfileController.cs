using Hms.Infrastructure.Cache;
using Hms.Service;
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

    public class UserProfileController : BaseApiController
    {
        #region Properties
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfileController> _logger;
        private readonly IConfiguration _configuration;
        #endregion

        #region  Constructor
        public UserProfileController(
            ICachedConfigurationService cachedConfigurationService,
            IConfiguration configuration,
            IUserProfileService userProfileService,
            ILogger<UserProfileController> logger
            ) : base(configuration, cachedConfigurationService)
        {
            _userProfileService = userProfileService;
            _logger = logger;
            _configuration = configuration;
        }
        #endregion

        #region UserProfile

        [HttpGet]
        [Route("user-profile")]
        public async Task<IActionResult> GetById(int userId)
        {
            var apiResponse = new ApiResponse();
            try
            {
                int loggedInUserId = Convert.ToInt32(this.User.Identity.GetUserId());
                int tenantId = Convert.ToInt32(this.User.Identity.GetTenantId());

                var response = await _userProfileService.GetUserProfileAsync(userId , tenantId);

                if (response != null)
                {
                    apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "User details retrived successfully.");
                }
                else
                {
                    apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Failed to retrive user details");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get the data of user: {Message}", ex.Message);
                apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Failed to retrive user details");
            }
            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}
