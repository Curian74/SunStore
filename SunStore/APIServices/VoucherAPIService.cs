using BusinessObjects.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SunStore.APIServices
{
    public class VoucherAPIService
    {
        private readonly HttpClient _httpClient;

        public VoucherAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<List<Voucher>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Voucher>>("Vouchers")
                   ?? new List<Voucher>();
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Voucher>($"Vouchers/{id}");
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateAsync(Voucher voucher)
        {
            var response = await _httpClient.PostAsJsonAsync("Vouchers", voucher);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                if (errorObj != null && errorObj.TryGetValue("message", out var msg))
                {
                    return (false, msg);
                }
            }
            return (false, "Unknown error occurred while creating voucher.");
        }

        public async Task<bool> UpdateAsync(int id, Voucher voucher)
        {
            var response = await _httpClient.PutAsJsonAsync($"Vouchers/{id}", voucher);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Vouchers/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
