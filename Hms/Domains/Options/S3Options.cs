namespace Hms.WebApi.Models.Options
{
    public class S3Options
    {
        public string? BucketName { get; set; }
        public int SignedUrlExpiresMinutes { get; set; } = 15;
    }

    public class S3PublicOptions
    {
        public string? BucketName { get; set; }
    }
}