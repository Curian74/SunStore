using System.Text;

namespace SunStoreAPI.Utils
{
    public class VerificationCodeGenerator
    {
        private static readonly string allowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string GenerateCode(int length = 6)
        {
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(allowedChars.Length);
                result.Append(allowedChars[index]);
            }

            return result.ToString();
        }
    }
}
