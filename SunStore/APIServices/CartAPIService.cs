using BusinessObjects.ApiResponses;
using BusinessObjects.DTOs;
using BusinessObjects.RequestModel;

namespace SunStore.APIServices
{
    public class CartAPIService
    {
        private readonly HttpClient _httpClient;

        public CartAPIService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public async Task<List<CartItemDto>> GetCartAsync(int customerId)
        {
            return await _httpClient.GetFromJsonAsync<List<CartItemDto>>($"Carts/{customerId}")
                ?? new List<CartItemDto>();
        }

        public async Task<ApiResult> AddToCartAsync(CartActionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Carts/add", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResult>();
            return result;
        }

        public async Task<CartInfoResponse> UpdateQuantityAsync(UpdateQuantityRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Carts/update-quantity", request);
            var result = await response.Content.ReadFromJsonAsync<CartInfoResponse>();
            return result;
        }

        public async Task<CartInfoResponse> DeleteItemAsync(int cartItemId)
        {
            var response = await _httpClient.DeleteAsync($"Carts/{cartItemId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CartInfoResponse>();
            return result ?? new CartInfoResponse { Total = 0 };
        }

    }

}
