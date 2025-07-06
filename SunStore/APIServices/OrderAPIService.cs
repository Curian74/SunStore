using BusinessObjects.ApiResponses;
using BusinessObjects.Models;

namespace SunStore.APIServices
{
    public class OrderAPIService
    {
        private readonly HttpClient _httpClient;

        public OrderAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<List<Order>> GetUnassignedOrders()
        {
            var response = await _httpClient.GetAsync("Orders/unassigned");

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
                return orders ?? new List<Order>();
            }

            else
            {
                // Read error content.
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed: {errorContent}");
            }
        }

        public async Task<List<Order>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("Orders");

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
                return orders ?? new List<Order>();
            }

            else
            {
                // Read error content.
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed: {errorContent}");
            }
        }

        public async Task<ApiResult<Order>?> GetOrderForShipperAssigning(int orderId, int shipperId)
        {
            var response = await _httpClient.GetAsync($"Orders/assigning?orderId={orderId}&shipperId={shipperId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResult<Order>>();

            return result;
        }

    }
}
