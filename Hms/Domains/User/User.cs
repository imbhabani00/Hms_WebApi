namespace Hms.Domains.User
{
    public class User
    {
        public int? Id { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public bool AgreeToTerms { get; set; }
        public int TenantId { get; set; }
        public int? UserId { get; set; }
        public string? RoleCode { get; set; }
        public string? ProfileCode { get; set; }
        public string? ColorCode { get; set; }
    }

    public class UserGet
    {
        public User? User { get; set; }
        public int? ReturnValue { get; set; }
    }
}
