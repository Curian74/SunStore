using BusinessObjects.ApiResponses;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using SunStore.ViewModel.DataModels;
using System.Net.Http.Json;

namespace SunStore.APIServices
{
    public class BillAPIService
    {
        private readonly HttpClient _httpClient;

        public BillAPIService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public async Task<List<Order>> GetBillsAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            string url = $"Bills/list?customerId={customerId}";

            if (fromDate.HasValue)
                url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";

            if (toDate.HasValue)
                url += $"&toDate={toDate.Value:yyyy-MM-dd}";

            return await _httpClient.GetFromJsonAsync<List<Order>>(url)
                ?? new List<Order>();
        }

        public async Task<BillDetailResponse?> GetBillDetailAsync(int billId)
        {
            return await _httpClient.GetFromJsonAsync<BillDetailResponse>($"Bills/detail/{billId}");
        }

        public async Task<BaseApiResponse> CancelBillAsync(int billId)
        {
            var response = await _httpClient.PostAsync($"Bills/cancel/{billId}", null);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();
            return result ?? new BaseApiResponse { IsSuccessful = false, Message = "Hủy đơn hàng thất bại" };
        }

        public async Task<string> GetCancelReasonAsync(int billId)
        {
            var res = await _httpClient.GetFromJsonAsync<CancelReasonDto>($"Bills/cancel-reason/{billId}");
            return res?.Reason ?? "Không có lý do";
        }
    }

    public class CancelReasonDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
