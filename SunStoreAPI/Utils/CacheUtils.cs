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
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(expirationInMinutes)); // Tự động xóa sau 10 phút

            _memoryCache.Set(cacheKey, code, cacheEntryOptions);
        }

        public bool VerifyResetCode(string email, string inputCode)
        {
            var cacheKey = $"reset_code:{email.ToLower()}";
            if (_memoryCache.TryGetValue(cacheKey, out string savedCode))
            {
                if (inputCode == savedCode)
                {
                    _memoryCache.Remove(cacheKey);
                    return true;
                }
            }

            return false; // Không tồn tại hoặc hết hạn
        }
    }
}
