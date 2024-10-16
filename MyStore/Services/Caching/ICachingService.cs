using Microsoft.Extensions.Caching.Memory;

namespace MyStore.Services.Caching
{
    public interface ICachingService
    {
        T? Get<T>(string cachekey);
        void Set<T>(string cachekey, T value, TimeSpan time);
        void Set<T>(string cachekey, T value, MemoryCacheEntryOptions options);
        void Remove(string cachekey);
    }
}
