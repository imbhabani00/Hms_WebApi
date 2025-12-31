using FluentValidation;
using Hms.Domains;
using Hms.Infrastructure.Cache;
using Hms.Service.Response;
using Hms.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hms.WebApi.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        private readonly ICachedConfigurationService _cachedConfigurationService1;
        private readonly IConfiguration _configuration;
        protected int? UserId { get; set; }
        protected int? TenantId { get; set; }
        protected int? RoleId { get; set; }

        public BaseApiController(IConfiguration configuration,
           ICachedConfigurationService cachedConfigurationService)
        {
            _cachedConfigurationService1 = cachedConfigurationService;
            _configuration = configuration;
        }

        protected void ResolveUserIdentity()
        {
            if (User?.Identity != null)
            {
                UserId = User.Identity.GetUserId();
                TenantId = User.Identity.GetTenantId();
                RoleId = User.Identity.GetUserRoleId();
            }
        }

        protected async Task<IActionResult> ValidateAndExecuteAsync<T>(
            IValidator<T> validator,
            T model,
            Func<T, Task<IActionResult>> action)
        {
            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(BuildValidationErrorApiResponse(validationResult));
            }
            return await action(model);
        }

        protected ApiResponse CreateSuccessApiResponse(object response = null,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string message = "Success")
        {
            return new ApiResponse
            {
                Response = response,
                ErrorMessage = "",
                StatusCode = (int)statusCode,
                Status = true,
                Message = message
            };
        }

        protected ApiResponse CreateFailedApiResponse(object? response = null,
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
            string errorMessage = "Failed")
        {
            return new ApiResponse
            {
                Response = response,
                ErrorMessage = errorMessage,
                StatusCode = (int)statusCode,
                Status = false,
                Message = "Failed"
            };
        }

        protected ApiResponse BuildValidationErrorApiResponse(FluentValidation.Results.ValidationResult validationResult)
        {
            var validationErrors = validationResult.Errors.Select(error => new ValidationError
            {
                ErrorCode = error.ErrorCode,
                ErrorMessage = error.ErrorMessage,
                PropertyName = error.PropertyName
            }).ToList();

            return new ApiResponse
            {
                Response = validationErrors,
                ErrorMessage = "Validation errors",
                StatusCode = 422,
                Status = false,
                Message = "Failed"
            };
        }
    }
}
