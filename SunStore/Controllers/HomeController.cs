using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Diagnostics;
using X.PagedList;
using SunStore.APIServices;

namespace SunStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ProductAPIService _productAPIService;

        public HomeController(ILogger<HomeController> logger, SunStoreContext context, ProductAPIService productAPIService)
        {
            _logger = logger;
            _context = context;
            _productAPIService = productAPIService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProductList(string? keyword, int? categoryID, string? priceRange, int? page)
        {
            var result = await _productAPIService.FilterAsync(keyword, categoryID, priceRange, page);
            
            ViewData["keyword"] = keyword;
            ViewData["categoryID"] = categoryID;
            ViewData["priceRange"] = priceRange;

            var categories = _context.Categories.ToList();
            ViewData["Categories"] = categories;

            return View(result);
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
