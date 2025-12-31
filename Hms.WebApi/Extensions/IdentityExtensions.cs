using System.Security.Claims;
using System.Security.Principal;

namespace Hms.WebApi.Extensions
{
    public static class IdentityExtensions
    {
        public const string TenantIdClaim = "http://schemas.microsoft.com/identity/claims/tenantid";

        public static int? GetUserId(this IIdentity identity)
        {
            var ident = identity as ClaimsIdentity;
            return ident?.FindFirst(ClaimTypes.NameIdentifier)?.Value != null
                ? int.TryParse(ident.FindFirst(ClaimTypes.NameIdentifier)!.Value, out var userId) ? userId : null
                : null;
        }

        public static int? GetTenantId(this IIdentity identity)
        {
            var ident = identity as ClaimsIdentity;
            return ident?.FindFirst(TenantIdClaim)?.Value != null
                ? int.TryParse(ident.FindFirst(TenantIdClaim)!.Value, out var tenantId) ? tenantId : null
                : null;
        }

        public static int? GetUserRoleId(this IIdentity identity)
        {
            var ident = identity as ClaimsIdentity;
            return ident?.FindFirst("RoleId")?.Value != null
                ? int.TryParse(ident.FindFirst("RoleId")!.Value, out var roleId) ? roleId : null
                : null;
        }

        public static UserContext GetUserContext(this ClaimsPrincipal principal)
        {
            return new UserContext
            {
                UserId = principal.Identity?.GetUserId(),
                TenantId = principal.Identity?.GetTenantId(),
                RoleId = principal.Identity?.GetUserRoleId()
            };
        }
    }

    public record UserContext
    {
        public int? UserId { get; init; }
        public int? TenantId { get; init; }
        public int? RoleId { get; init; }
    }

}