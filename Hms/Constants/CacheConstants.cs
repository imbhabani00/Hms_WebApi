namespace Hms.Constants
{
    internal class CacheConstants
    {
        public const string RedisCache = "redisCache";
        public const string InMemoryCache = "in-memory-cache";

        // Cache keys prefix
        public const string AccessCodePrefix = "AccessCode_";
        public const string UserSessionPrefix = "UserSession_";
        public const string TenantConfigPrefix = "TenantConfig_";
    }
}
