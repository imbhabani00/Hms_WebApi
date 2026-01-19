using Hms.Infrastructure.Cache;
using Hms.Service;
using Hms.Service.Request.Users;
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

    public class UserController : BaseApiController
    {
        #region Properties
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        #endregion

        #region  Constructor
        public UserController(
            ICachedConfigurationService cachedConfigurationService,
            IConfiguration configuration,
            IUserService userService,
            ILogger<UserController> logger
            ) : base(configuration, cachedConfigurationService)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
        }
        #endregion

        #region Register
        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRequest userRequest)
        {
            var apiResponse = new ApiResponse();
            try
            {
                var response = await _userService.RegisterAsync(userRequest, 0, 0);
                switch (response.ReturnValue)
                {
                    case 0:
                        if (response.NewId > 0)
                        {
                            apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "Registration successful! Please check your email for access code.");
                        }
                        else
                        {
                            apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "User details updated successfully.");
                        }
                        break;
                    case 1:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.NotFound, "Email already exists. Please use a different email.");
                        break;
                    default:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Registration failed. Please try again.");
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving the user details.{email}", ex.Message);
                apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
            return new ObjectResult(apiResponse);
        }
        #endregion

        #region UpdateAccessCode
        [HttpPut]
        [Route("update-access-code")]
        public async Task<IActionResult> UpdateAccessCode()
        {
            ApiResponse apiResponse = null;
            try
            {
                ResolveUserIdentity();
                int loggedInUserId = Convert.ToInt32(this.User.Identity.GetUserId());
                int tenantId = Convert.ToInt32(this.User.Identity.GetTenantId());

                var response = await _userService.UpdateUserAccessCode(loggedInUserId, tenantId);

                if (response == 0)
                {
                    apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "Access code sent successfully");
                }
                else
                {
                    apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Failed to send access code");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Update access code error: {Message}", ex.Message);
                apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Internal server error");
            }

            return new ObjectResult(apiResponse);
        }
        #endregion

        #region ValidateAccessCode
        [HttpGet]
        [Route("validate-access-code")]
        public async Task<IActionResult> ValidateAccessCode(string accessCode)
        {
            ApiResponse apiResponse = null;
            try
            {
                ResolveUserIdentity();
                int loggedInUserId = Convert.ToInt32(this.User.Identity.GetUserId());
                int timeValid = _configuration.GetValue<int>("AccessCodeValidForSeconds");

                var response = await _userService.ValidateAccessCode(loggedInUserId, timeValid, accessCode);

                switch (response.ReturnValue)
                {
                    case 0:
                        apiResponse = CreateSuccessApiResponse(response, HttpStatusCode.OK, "Access code validated successfully");
                        break;
                    case 1:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.BadRequest, "Invalid access code");
                        break;
                    case 2:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.BadRequest, "User not found");
                        break;
                    case 3:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.BadRequest, "Access code expired");
                        break;
                    default:
                        apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Validation failed");
                        break;
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError("Validate access code error: {Message}", ex.Message);
                apiResponse = CreateFailedApiResponse(null, HttpStatusCode.InternalServerError, "Internal server error");
            }

            return new ObjectResult(apiResponse);
        }
        #endregion
    }
}
