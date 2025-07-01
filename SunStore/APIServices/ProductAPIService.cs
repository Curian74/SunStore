using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using SunStore.ViewModel;
using SunStore.ViewModel.RequestModels;
using System.Drawing.Printing;
using System.Net.Http.Headers;

namespace SunStore.APIServices
{
    public class ProductAPIService
    {
        private readonly HttpClient _httpClient;

        public ProductAPIService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<PagedResult<Product>> FilterAsync(string? keyword, int? categoryID, string? priceRange, int? page,
            int? pageSize)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(keyword)) queryParams.Add($"keyword={keyword}");
            if (categoryID.HasValue) queryParams.Add($"categoryID={categoryID}");
            if (!string.IsNullOrEmpty(priceRange)) queryParams.Add($"priceRange={priceRange}");
            if (page.HasValue) queryParams.Add($"page={page}");
            if (pageSize.HasValue) queryParams.Add($"pageSize={pageSize}");

            var url = "Products/filter";
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            var pagedResult = await _httpClient.GetFromJsonAsync<PagedResult<Product>>(url);
            return pagedResult ?? new PagedResult<Product>();
        }

        public async Task<ApiResult<string>?> UploadImageAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            if (file != null)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.FileName);
            }

            var response = await _httpClient.PostAsync("Products/image", content);

            var result = await response.Content.ReadFromJsonAsync<ApiResult<string>>();

            return result;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var data = await _httpClient.GetFromJsonAsync<Product>($"Products/{id}");

            return data;
        }

        public async Task<BaseApiResponse?> CreateAsync(CreateProductRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("Products", model);

            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }

        public async Task<BaseApiResponse?> EditAsync(EditProductRequestViewModel model, int pId)
        {
            var response = await _httpClient.PutAsJsonAsync($"Products/{pId}", model);

            var result = await response.Content.ReadFromJsonAsync<BaseApiResponse>();

            return result;
        }
    }
}
