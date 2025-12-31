namespace Hms.WebApi.Models
{
    public class ForgotPasswordModel
    {
        public string Email { get; set; } = string.Empty;
        public int? TenantId { get; set; }
    }
}
