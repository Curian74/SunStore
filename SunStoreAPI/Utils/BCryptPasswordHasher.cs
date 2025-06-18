using BusinessObjects.Models;
using Humanizer;

namespace SunStoreAPI.Utils
{
    public static class BCryptPasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string requestPassword, string currentPassword)
        {
            return BCrypt.Net.BCrypt.Verify(requestPassword, currentPassword);
        }
    }
}
