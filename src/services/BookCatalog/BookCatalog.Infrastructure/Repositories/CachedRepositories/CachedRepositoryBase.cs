using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace BookCatalog.Infrastructure.Repositories;

    public abstract class CachedRepositoryBase
    {
        private readonly IDistributedCache _cache;
        protected virtual TimeSpan CacheLifetime => TimeSpan.FromMinutes(5);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    
        protected CachedRepositoryBase(IDistributedCache cache)
        {
            _cache = cache;
        }

        protected async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return JsonSerializer.Deserialize<T>(value, JsonOptions);
            }

            var obj = await factory();

            await SetAsync(key, obj);

            return obj;
        }

        protected async Task SetAsync<T>(string key, T obj)
        {
            var value = JsonSerializer.Serialize(obj, JsonOptions);

            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = CacheLifetime
            });
        }

        protected async Task RemoveCache(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        protected string GetCacheKey(string key, params object[] properties)
        {
            return $"{key};{(properties.Length > 0 ? string.Join(";", properties) : string.Empty)}";
        }
}