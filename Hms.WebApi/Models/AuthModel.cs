namespace Hms.WebApi.Models
{
    public class AuthModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int TenantId { get; set; }
    }
}
