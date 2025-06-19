using Microsoft.Extensions.Caching.Memory;

namespace SunStoreAPI.Utils
{
    public class CacheUtils
    {
        private readonly IMemoryCache _memoryCache;

        public CacheUtils(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void SaveResetCode(string email, string code, int expirationInMinutes)
        {
            var cacheKey = $"reset_code:{email.ToLower()}";

            // Expires after 10 mins
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(expirationInMinutes));

            _memoryCache.Set(cacheKey, code, cacheEntryOptions);
        }

        public bool VerifyResetCode(string email, string inputCode)
        {
            var cacheKey = $"reset_code:{email.ToLower()}";
            if (_memoryCache.TryGetValue(cacheKey, out string savedCode))
            {
                return inputCode == savedCode;
            }

            // Invalid or expired code.
            return false;
        }

        public void RemoveResetCode(string email)
        {
            var cacheKey = $"reset_code:{email.ToLower()}";
            _memoryCache.Remove(cacheKey);
        }
    }
}
