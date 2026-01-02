namespace Hms.WebApi.Models
{
    public class AuthModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int TenantId { get; set; }
        public string? ReturnUrl { get; set; }
        public string? RememberMe { get; set; }
    }
}
