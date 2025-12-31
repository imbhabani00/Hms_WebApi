using PasswordGenerator;

namespace Hms.Helpers
{
    public class PasswordHelper
    {
        public string GenerateSecurePassword(int length = 12)
        {
            var passwordObj = new Password(
                includeLowercase: true,
                includeUppercase: true,
                includeNumeric: true,
                includeSpecial: true,
                passwordLength: length
            );
            return passwordObj.Next();
        }

        public string GenerateSimplePassword()
        {
            var passwordObj = new Password(
                includeLowercase: true,
                includeUppercase: true,
                includeNumeric: true,
                includeSpecial: false,
                passwordLength: 8
            );
            return passwordObj.Next();
        }

        public bool IsStrongPassword(string password)
        {
            return password.Length >= 12 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}
