using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using SunStore.ViewModel;
using System.Drawing.Printing;

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

    }
}
