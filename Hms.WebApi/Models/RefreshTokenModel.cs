namespace Hms.WebApi.Models
{
    public class RefreshTokenModel
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }
}
