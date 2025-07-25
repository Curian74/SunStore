using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using SunStore.ViewModel.DataModels;
using SunStore.ViewModel.RequestModels;
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

        public async Task<PagedResult<Voucher>?> GetPagedAsync(int? page = 1, int? pageSize = 7)
        {
            var url = $"Vouchers/filtered?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<Voucher>>();

            return result;
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Voucher>($"Vouchers/{id}");
        }

        public async Task<BaseApiResponse?> CreateAsync(CreateVoucherViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Vouchers", model);

            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<BaseApiResponse?> UpdateAsync(EditVoucherViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"Vouchers/{model.VoucherId}", model);

            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Vouchers/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
