using BusinessObjects.ApiResponses;
using SunStore.Extensions;
using SunStore.ViewModel.DataModels;
using SunStore.ViewModel.RequestModels;
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

        public async Task<BaseApiResponse?> RegisterAsync(RegisterRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/register", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<bool> SignOutAsync()
        {
            var response = await _httpClient.PostAsync("Auth/logout", null);

            return response.IsSuccessStatusCode;
        }

        public async Task<BaseApiResponse?> SendResetPasswordRequestAsync(ResetPasswordRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/password", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<BaseApiResponse?> SendOTPVerificationRequestAsync(OTPVerificationRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/verify-reset", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<BaseApiResponse?> UpdatePasswordAsync(UpdatePasswordRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/reset-password", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<BaseApiResponse?> GetProfileInfoAsync()
        {
            var response = await _httpClient.GetAsync("Auth/me");

            var result = await response.Content.ReadFromJsonAsync<ApiResult<UserViewModel>>();

            return result;
        }
    }
}
