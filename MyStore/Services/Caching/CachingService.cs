
using Microsoft.Extensions.Caching.Memory;

namespace MyStore.Services.Caching
{
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _memoryCache;
        public CachingService(IMemoryCache memoryCache) => _memoryCache = memoryCache;

        public T? Get<T>(string cachekey)
        {
           _memoryCache.TryGetValue(cachekey, out T? value);
            return value;
        }

        public void Set<T>(string cachekey, T value, TimeSpan time)
        {
            _memoryCache.Set(cachekey, value, time);
        }
        
        public void Remove(string cachekey)
        {
            _memoryCache.Remove(cachekey);
        }

        public void Set<T>(string cachekey, T value, MemoryCacheEntryOptions options)
        {
            _memoryCache.Set(cachekey, value, options);
        }
    }
}
