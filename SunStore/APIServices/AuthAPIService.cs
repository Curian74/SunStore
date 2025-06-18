using BusinessObjects.ApiResponses;
using SunStore.Extensions;
using SunStore.Models;
using SunStore.ViewModel;
using System.Text.Json;

namespace SunStore.APIServices
{
    public class AuthAPIService
    {
        private readonly HttpClient _httpClient;

        public AuthAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<LoginResponseModel> LoginAsync(LoginRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();

                return result ?? new LoginResponseModel();
            }

            else
            {
                // Read error content.
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorObj = JsonDeserializationExtension.Deserialize<LoginResponseModel>(errorContent);

                    var errorMessage = errorObj?.ErrorMessage ?? "Đã xảy ra lỗi không xác định.";
                    throw new HttpRequestException(errorMessage);
                }

                catch (JsonException)
                {
                    throw new HttpRequestException("Lỗi không xác định.");
                }
            }
        }

        public async Task<ApiResult?> RegisterAsync(RegisterRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/register", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResult>();

            return result;
        }

        public async Task<bool> SignOutAsync()
        {
            var response = await _httpClient.PostAsync("Auth/logout", null);

            return response.IsSuccessStatusCode;
        }
    }
}
