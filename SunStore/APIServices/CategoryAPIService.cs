using BusinessObjects.Models;
using SunStore.Extensions;
using System.Net.Http;
using System.Text.Json;

namespace SunStore.APIServices
{
    public class CategoryAPIService
    {
        private readonly HttpClient _httpClient;

        public CategoryAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<List<Category>> GetAllAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("Categories");

            string result = await response.Content.ReadAsStringAsync();

            var categories = JsonDeserializationExtension.Deserialize<List<Category>>(result);

            return categories;
        }
    }
}
