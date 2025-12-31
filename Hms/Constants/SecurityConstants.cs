namespace Hms.Constants
{
    public static class SecurityConstants
    {
        // JWT Settings
        public const string JWTSecretKey = "JWTSettings:JWTSecretKey";

        // SendGrid Email Service
        public const string SendGridApiKey = "SendGridApiKey";

        // Authentication & Session
        public const string CookieExpiration = "CookieExpiration";
        public const string AccessCodeValidTimeInSeconds = "AccessCodeValidTimeInSeconds";

        // AWS Configuration
        public const string AWSAccessKey = "AWS:Credentials:AccessKey";
        public const string AWSSecretKey = "AWS:Credentials:SecretKey";
        public const string AWSRegion = "AWS:Region";

        // AWS S3 Bucket 
        public const string AWSPrivateBucketName = "AWS:S3:Private:BucketName";
        public const string AWSPublicBucketName = "AWS:S3:Public:BucketName";
    }
}