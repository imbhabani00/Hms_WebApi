using EasyCaching.Core;
using Hms.Constants;
using Microsoft.Extensions.Configuration;

namespace Hms.Infrastructure.Cache
{
    public interface ICachedConfigurationService
    {
        Task SetCachedValue<T>(string keyName, T value,
            string defaultProviderName = CacheConstants.InMemoryCache);
        Task<T> GetCachedValue<T>(string keyName,
            string defaultProviderName = CacheConstants.InMemoryCache);
    }
    public class CachedConfigurationService : ICachedConfigurationService
    {
        private readonly TimeSpan defaultCachEventme;
        private readonly IConfiguration configuration;
        private readonly IEasyCachingProviderFactory cachingProviderFactory;

        public CachedConfigurationService(IConfiguration configuration,
            IEasyCachingProviderFactory cachingProviderFactory)
        {
            defaultCachEventme = TimeSpan.FromDays(1);
            this.configuration = configuration;
            this.cachingProviderFactory = cachingProviderFactory;
        }

        public async Task SetCachedValue<T>(string keyName, T value,
            string defaultProviderName = CacheConstants.InMemoryCache)
        {
            if (cachingProviderFactory == null)
                return;

            var provider = cachingProviderFactory.GetCachingProvider(defaultProviderName);
            await provider.SetAsync(keyName, value,
               defaultCachEventme);
        }

        public async Task<T> GetCachedValue<T>(string keyName,
            string defaultProviderName = CacheConstants.InMemoryCache)
        {
            if (cachingProviderFactory == null)
                return default(T);

            string cacheKey = string.Format("{0}", keyName);
            var provider = cachingProviderFactory.GetCachingProvider(defaultProviderName);
            var cacheValue = await provider.GetAsync<T>(cacheKey);

            if (cacheValue.HasValue)
            {
                return cacheValue.Value;
            }

            return default(T);
        }
    }
}
