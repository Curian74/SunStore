using BusinessObjects;
using System.Text;
using BusinessObjects.Models;

namespace SunStore.APIServices
{
    public class NotificationAPIService
    {
        private readonly HttpClient _httpClient;

        public NotificationAPIService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public async Task<PagedResult<Notification>?> GetPaged(string? userId,
            int? pageIndex = 1, int? pageSize = 7)
        {
            var apiUrl = "Notification";

            var sb = new StringBuilder(apiUrl);

            var paramsList = new List<string>();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                paramsList.Add($"userId={userId}");
            }
           
            paramsList.Add($"pageIndex={pageIndex.ToString()}");
            paramsList.Add($"pageSize={pageSize.ToString()}");

            if (paramsList.Count > 0)
            {
                sb.Append('?').Append(string.Join('&', paramsList));
            }

            var response = await _httpClient.GetAsync(sb.ToString());

            var result = await response.Content.ReadFromJsonAsync<PagedResult<Notification>>();

            return result;
        }
    }
}
