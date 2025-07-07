using BusinessObjects.Models;
using System.Net.Http.Json;

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

        public async Task<bool> CreateAsync(Voucher voucher)
        {
            var response = await _httpClient.PostAsJsonAsync("Vouchers", voucher);
            return response.IsSuccessStatusCode;
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
