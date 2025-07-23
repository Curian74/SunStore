using BusinessObjects.ApiResponses;
using BusinessObjects.RequestModel;
using Microsoft.AspNetCore.Http;
using SunStore.ViewModel.DataModels;
using SunStore.ViewModel.RequestModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace SunStore.APIServices
{
    public class CheckoutAPIService
    {
        private readonly HttpClient _httpClient;

        public CheckoutAPIService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public async Task<CheckoutInitResponse?> GetCheckoutInitInfoAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"Checkout/init-checkout/{userId}");

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<CheckoutInitResponse>();
            return result;
        }


        public async Task<ApiResult<JsonElement>> CreateOrderAsync(OrderRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Checkout/CreateBill", model);
            var json = await response.Content.ReadAsStringAsync();

            var result = new ApiResult<JsonElement>
            {
                IsSuccessful = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Success" : "Failed",
                Data = JsonDocument.Parse(json).RootElement
            };

            return result;
        }

        public async Task<(bool IsSuccessful, JsonElement Data)> CreateDepositVNPayAsync(OrderRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Checkout/Deposit", model);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                return (true, json);
            }

            return (false, default);
        }


        // Gọi API thanh toán VNPay
        public async Task<string> GetVnPayUrlAsync(VnPayRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Checkout/vnpay-url", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<VoucherCheckResponse> UseVoucherAsync(string code, int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<VoucherCheckResponse>(
                $"checkout/use-voucher?code={code}&userId={userId}");

            return response ?? new VoucherCheckResponse();
        }
    }
}
