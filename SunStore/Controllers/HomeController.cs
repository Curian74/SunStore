using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Diagnostics;
using X.PagedList;

namespace SunStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, SunStoreContext context)
        {
            _logger = logger;
            _context = context; 
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProductList(string? keyword, int? categoryID, string? priceRange, int? page)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
                if (keyword.Length > 100)
                {
                    ModelState.AddModelError("Keyword", "Từ khóa tìm kiếm không được vượt quá 100 ký tự.");
                    keyword = null;
                }
                //keyword = RemoveDiacritics(keyword);
            }

            var productQuery = _context.Products.Include(b => b.Category)
                                             .Include(b => b.ProductOptions);

            var productList = await productQuery.ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                productList = productList.Where(b => b.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (categoryID.HasValue)
            {
                productList = productList.Where(b => b.CategoryId == categoryID.Value).ToList();
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceParts = priceRange.Split('-');
                if (priceParts.Length == 2)
                {
                    if (Decimal.TryParse(priceParts[0], out decimal minPrice) && Decimal.TryParse(priceParts[1], out decimal maxPrice))
                    {
                        productList = productList.Where(b => b.ProductOptions.Any(bo => (decimal)bo.Price >= minPrice && (decimal)bo.Price <= maxPrice)).ToList();
                    }
                }
                else if (priceRange.EndsWith("+") && Decimal.TryParse(priceRange.TrimEnd('+'), out decimal minPriceOnly))
                {
                    productList = productList.Where(b => b.ProductOptions.Any(bo => (decimal)bo.Price >= minPriceOnly)).ToList();
                }
            }

            //var categories = await _context.Category.ToListAsync();
            //ViewData["Categories"] = categories;

            int pageSize = 9;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            PagedList<Product> lst = new PagedList<Product>(productList, pageNumber, pageSize);
            ViewData["keyword"] = keyword;
            ViewData["categoryID"] = categoryID;
            ViewData["priceRange"] = priceRange;

            var categories = _context.Categories.ToList();
            ViewData["Categories"] = categories;

            if (!ModelState.IsValid)
            {
                return View("Error");
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductListPartial", lst);
            }

            return View(lst);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
