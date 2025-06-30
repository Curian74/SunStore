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
        private readonly ProductOptionAPIService _optionAPIService;

        public HomeController(ILogger<HomeController> logger, SunStoreContext context, ProductAPIService productAPIService, ProductOptionAPIService optionAPIService)
        {
            _logger = logger;
            _context = context;
            _productAPIService = productAPIService;
            _optionAPIService = optionAPIService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProductList(string? keyword, int? categoryID, string? priceRange, int? page)
        {
            var result = await _productAPIService.FilterAsync(keyword, categoryID, priceRange, page, 9);
            
            ViewData["keyword"] = keyword;
            ViewData["categoryID"] = categoryID;
            ViewData["priceRange"] = priceRange;

            var categories = _context.Categories.ToList();
            ViewData["Categories"] = categories;

            return View(result);
        }

        public async Task<IActionResult> ProductDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _optionAPIService.GetDetail((int)id);

            if (result == null)
            {
                return NotFound();
            }

            //var listOptions = _optionAPIService.GetAll();
            //ViewData["Options"] = listOptions;

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
