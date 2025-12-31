namespace Hms.Constants
{
    public static class ApiConstants
    {
        public const string ApiVersion = "2.0";
        public const string BaseRoute = "api/v{version:apiVersion}";

        // Rate limiting policies
        public const string ForgotPasswordPolicy = "forgotPassword";
        public const string AccessCodePolicy = "accessCode";
    }
}
