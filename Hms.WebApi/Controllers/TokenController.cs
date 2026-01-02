using Hms.Constants;
using Hms.Helpers;
using Hms.Infrastructure.Cache;
using Hms.Service;
using Hms.Service.Response;
using Hms.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;

namespace Hms.WebApi.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TokenController : BaseApiController
    {
        #region Properties
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        #endregion

        #region Constructor
        public TokenController(IUserService userService,
            ITokenService tokenService,
            IConfiguration configuration,
            ICachedConfigurationService cachedConfigurationService,
            ILogger<TokenController> logger) :
            base(configuration, cachedConfigurationService)
        {
            _userService = userService;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }
        #endregion

        #region AccessToken
        [HttpPost]
        [AllowAnonymous]
        [Route("access-token")]
        public async Task<IActionResult> AccessToken([FromBody] AuthModel authModel)
        {
            ApiResponse apiResponse;

            try
            {
                ResolveUserIdentity();

                // Encrypt password
                string bsmKey = _configuration.GetValue<string>("BSMSecret");
                string encryptedPassword = BSMEncryptDecryptHelper.Encrypt(authModel.Password, bsmKey, 256);
                var tenantId = authModel.TenantId;

                // Validate user
                var response = await _userService.Validate(
                    authModel.Email,
                    encryptedPassword,
                    tenantId
                );

                switch (response.ReturnValue)
                {
                    case 0:
                        if (response.User == null)
                        {
                            apiResponse = CreateFailedApiResponse(
                                null,
                                HttpStatusCode.InternalServerError,
                                "User details not found."
                            );
                            break;
                        }

                        var expires = DateTime.UtcNow.AddMinutes(
                            _configuration.GetValue<int>(
                                "JWTSettings:AccessTokenExpiryInMinutes"
                            )
                        );

                        var accessToken =
                            _tokenService.GenerateAccessToken(
                                response.User,
                                expires
                            );

                        var refreshToken =
                            _tokenService.GenerateRefreshToken(response.User);

                        apiResponse = CreateSuccessApiResponse(
                            new
                            {
                                token = accessToken,
                                refreshToken = refreshToken,
                                expires = expires,
                                userId = response.User.UserId,
                                email = response.User.Email,
                                firstName = response.User.FirstName,
                                lastName = response.User.LastName,
                                roleId = response.User.RoleId,
                                roleName = response.User.RoleName,
                                tenantId = response.User.TenantId
                            },
                            HttpStatusCode.OK
                        );
                        break;

                    case 1:
                        apiResponse = CreateFailedApiResponse(response, HttpStatusCode.Unauthorized, "Your account is inactive.");
                        break;
                    case 2:
                        apiResponse = CreateFailedApiResponse(response, HttpStatusCode.Unauthorized, "Invalid password.");
                        break;
                    case 3:
                        apiResponse = CreateFailedApiResponse(response, HttpStatusCode.Unauthorized, "Account has been deleted.");
                        break;
                    case 4: 
                        apiResponse = CreateFailedApiResponse(response, HttpStatusCode.Unauthorized, "Email address not found.");
                        break;

                    default:
                        apiResponse = CreateFailedApiResponse(
                            response,
                            HttpStatusCode.InternalServerError,
                            "An unexpected error occurred."
                        );
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AccessToken generation");
                apiResponse = CreateFailedApiResponse(
                    null,
                    HttpStatusCode.InternalServerError,
                    "Internal server error"
                );
            }

            return Ok(apiResponse);
        }
        #endregion

        #region RefreshToken

        [HttpPost]
        [AllowAnonymous]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            ApiResponse apiResponse;

            try
            {
                // Get principal from expired access token
                var principal =
                    _tokenService.GetPrincipalFromExpiredToken(model.Token);

                var userIdClaim =
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var tenantIdClaim =
                    principal.FindFirst(CustomClaimTypesConstants.TenantId)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) ||
                    string.IsNullOrEmpty(tenantIdClaim))
                {
                    return Unauthorized("Invalid token");
                }

                int userId = Convert.ToInt32(userIdClaim);
                int tenantId = Convert.ToInt32(tenantIdClaim);

                // Get user from DB
                var userResponse =
                    await _userService.GetByUserId(userId, tenantId);

                if (userResponse.ReturnValue != 0 ||
                    userResponse.User == null)
                {
                    return Unauthorized("User not found");
                }

                // Generate new access token
                var expires = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>(
                        "JWTSettings:AccessTokenExpiryInMinutes"
                    )
                );

                var newAccessToken =
                    _tokenService.GenerateAccessToken(
                        userResponse.User,
                        expires
                    );

                apiResponse = CreateSuccessApiResponse(
                    new
                    {
                        token = newAccessToken,
                        refreshToken = model.RefreshToken,
                        expires = expires,
                        currentServerTime = DateTime.UtcNow
                    },
                    HttpStatusCode.OK
                );
            }
            catch (SecurityTokenException)
            {
                apiResponse = CreateFailedApiResponse(
                    null,
                    HttpStatusCode.Unauthorized,
                    "Invalid token"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RefreshToken");
                apiResponse = CreateFailedApiResponse(
                    null,
                    HttpStatusCode.InternalServerError,
                    "Internal server error"
                );
            }

            return Ok(apiResponse);
        }
        #endregion
    }
}
