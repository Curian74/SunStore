using BusinessObjects.ApiResponses;
using SunStore.ViewModel.RequestModels;

namespace SunStore.APIServices
{
    public class UserAPIService
    {
        private readonly HttpClient _httpClient;

        public UserAPIService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("api");
        }

        public async Task<BaseApiResponse?> UpdateUserAsync(UpdateUserRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("User/update", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }
    }
}
