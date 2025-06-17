using System.Text.Json;

namespace SunStore.Extensions
{
    public static class JsonDeserializationExtension
    {
        private static readonly JsonSerializerOptions defaultSettings = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        // Goi thang nay moi khi can Deserialize
        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, defaultSettings)!;
        }
    }
}
