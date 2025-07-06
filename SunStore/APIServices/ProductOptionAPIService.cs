using BusinessObjects.Models;
using BusinessObjects;
using BusinessObjects.ApiResponses;
using SunStore.ViewModel.RequestModels;

namespace SunStore.APIServices
{
    public class ProductOptionAPIService
    {
        private readonly HttpClient _httpClient;

        public ProductOptionAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<List<ProductOption>> GetAll()
        {
            var url = "ProductOptions";

            var result = await _httpClient.GetFromJsonAsync<List<ProductOption>>(url);
            return result ?? new List<ProductOption>();
        }

        public async Task<ProductDetailResponse> GetDetail(int id)
        {
            var url = $"ProductOptions/detail/{id}";

            var result = await _httpClient.GetFromJsonAsync<ProductDetailResponse>(url);
            return result ?? new ProductDetailResponse();
        }

        public async Task<BaseApiResponse?> CreateAsync(CreateProductOptionRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("ProductOptions", model);

            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }
    }
}
