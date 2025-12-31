namespace Hms.Service.Request.Email
{
    public class AccessCodeEmailModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AccessCode { get; set; }
        public int ValidMinutes { get; set; }
        public string? LoginUrl { get; set; }
    }
    public class WelcomeEmailModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? LoginUrl { get; set; }
    }
}
