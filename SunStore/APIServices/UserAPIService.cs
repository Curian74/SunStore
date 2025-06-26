using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Queries;
using SunStore.ViewModel.DataModels;
using SunStore.ViewModel.RequestModels;
using System.Text;

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
        
        public async Task<BaseApiResponse?> CreateUserAsync(CreateUserRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("User/create", model);
            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<ApiResult<UserViewModel>?> GetUserByIdAsync(int? id)
        {
            var response = await _httpClient.GetAsync($"User/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResult<UserViewModel>>();

            return result;
        }

        public async Task<ApiResult<PagedResult<UserViewModel>>?> GetPagedUserAsync(UserQueryObject queryObject)
        {
            var apiUrl = "User/filter";

            var sb = new StringBuilder(apiUrl);

            var paramsList = new List<string>();

            paramsList.Add($"CurrentPage={queryObject.CurrentPage.ToString()}");
            paramsList.Add($"PageSize={queryObject.PageSize.ToString()}");

            if (paramsList.Count > 0)
            {
                sb.Append('?').Append(string.Join('&', paramsList));
            }

            var response = await _httpClient.GetAsync(sb.ToString());

            var result = await response.Content.ReadFromJsonAsync<ApiResult<PagedResult<UserViewModel>>>();

            return result;
        }
    }
}
