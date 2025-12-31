using Microsoft.Extensions.Configuration;

namespace Hms.Helpers
{
    public class AccessCodeHelper
    {
        private readonly IConfiguration _config;

        public AccessCodeHelper(IConfiguration config)
        {
            _config = config;
        }

        public int GenerateSecurityCode(int startDigit = 100000, int endDigit = 999999)
        {
            var random = new Random();
            return random.Next(startDigit, endDigit);
        }
    }
}
