using Hms.Service.Response.Users;

namespace Hms.Domains.UserProfile
{
    public class UserProfile
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? ProfilePictureName { get; set; }
        public string? ProfilePicturePath { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastLogoutDate { get; set; }
        public bool AgreeToTerms { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
    }

    public class UserProfileGet
    {
        public UserProfile? User { get; set; }
        public int? ReturnValue { get; set; }
    }
}
