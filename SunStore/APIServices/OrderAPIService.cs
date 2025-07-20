using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using SunStore.ViewModel.DataModels;
using System.Text.Json;

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

        public async Task<PagedResult<Order>> GetOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 6)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            string url = $"Orders/list?{string.Join("&", queryParams)}";

            var result = await _httpClient.GetFromJsonAsync<PagedResult<Order>>(url);
            return result ?? new PagedResult<Order>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = 0,
                Items = new List<Order>()
            };
        }

        public async Task<BillDetailResponse?> GetOrderDetailAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Orders/detail/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BillDetailResponse>();
            }
            return null;
        }

        public async Task<BaseApiResponse> CancelOrderAsync(int id, string reason = "default")
        {
            var response = await _httpClient.PostAsync($"Orders/cancel/{id}?reason={Uri.EscapeDataString(reason)}",null);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();
            return result ?? new BaseApiResponse { IsSuccessful = false, Message = "Unknown error" };
        }

        public async Task<string> GetCancelReasonAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Orders/cancel-reason/{id}");
            if (response.IsSuccessStatusCode)
            {
                var obj = await response.Content.ReadFromJsonAsync<JsonElement>();
                return obj.GetProperty("reason").GetString() ?? "Không có lý do";
            }
            return "Không có lý do";
        }

        public async Task<PagedResult<Order>> GetShipperPendingOrdersAsync(int shipperId, int page = 1,
            int pageSize = 5)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            string url = $"Orders/shipper-pending/{shipperId}?{string.Join("&", queryParams)}";

            var result = await _httpClient.GetFromJsonAsync<PagedResult<Order>>(url);

            return result ?? new PagedResult<Order>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = 0,
                Items = new List<Order>()
            };
        }
    }
}
