using BusinessObjects.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SunStore.APIServices;
using SunStore.ViewModel.RequestModels;

namespace SunStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductAPIService _productAPIService;
        private readonly CategoryAPIService _categoryAPIService;
        private readonly ProductOptionAPIService _optionAPIService;

        public ProductsController(ProductAPIService productAPIService, CategoryAPIService categoryAPIService, 
            ProductOptionAPIService optionAPIService)
        {
            _productAPIService = productAPIService;
            _categoryAPIService = categoryAPIService;
            _optionAPIService = optionAPIService;
        }

        // GET: Products
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Index(string? keyword, int? categoryID, string? priceRange, int? page,
            int? pageSize = 8)
        {
            var products = await _productAPIService.FilterAsync(keyword, categoryID, priceRange, page, pageSize);

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productAPIService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            var productOption = await _optionAPIService.GetDetail(id);
            if (productOption == null)
            {
                return NotFound();
            }

            var userID = HttpContext!.Session.GetString("UserId");
            ViewBag.userID = userID;

            ViewData["Quantity"] = productOption.Quantity;
            ViewData["Price"] = productOption.Price;
            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryAPIService.GetAllAsync();

            var vm = new CreateProductRequestViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }).ToList()
            };

            return View(vm);
        }

        //// POST: Products/Create
        [HttpPost]
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Create(CreateProductRequestViewModel model, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Create), model);
            }

            try
            {
                if (image != null)
                {
                    var uploadImageResult = await _productAPIService.UploadImageAsync(image);

                    if (uploadImageResult!.IsSuccessful)
                    {
                        model.ImageUrl = uploadImageResult.Data;
                    }
                }

                var result = await _productAPIService.CreateAsync(model);

                if (result!.IsSuccessful)
                {
                    TempData["success"] = result.Message;
                }

                else
                {
                    TempData["error"] = result.Message;
                }

                return RedirectToAction(nameof(Create));
            }

            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        //// GET: Products/Edit/5
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productAPIService.GetByIdAsync(id);

            var categories = await _categoryAPIService.GetAllAsync();

            var vm = new EditProductRequestViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }).ToList(),
                CategoryId = product.CategoryId,
                Description = product.Description,
                ImageUrl = product.Image,
                IsDeleted = product.IsDeleted,
                Name = product.Name,
            };

            return View(vm);
        }

        //// POST: Products/Edit/5
        [HttpPost]
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Edit(int id, EditProductRequestViewModel model, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            try
            {
                if (image != null)
                {
                    var uploadImageResult = await _productAPIService.UploadImageAsync(image);

                    if (uploadImageResult!.IsSuccessful)
                    {
                        model.ImageUrl = uploadImageResult.Data;
                    }
                }

                var result = await _productAPIService.EditAsync(model, id);

                if (result!.IsSuccessful)
                {
                    TempData["success"] = result.Message;
                }

                else
                {
                    TempData["error"] = result.Message;
                }

                return RedirectToAction(nameof(Edit), new { id });
            }

            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        //// GET: Products/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .Include(p => p.Category)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //// POST: Products/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product != null)
        //    {
        //        _context.Products.Remove(product);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ProductExists(int id)
        //{
        //    return _context.Products.Any(e => e.Id == id);
        //}
    }
}
