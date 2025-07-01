using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using SunStore.APIServices;
using SunStore.ViewModel.RequestModels;

namespace SunStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductAPIService _productAPIService;
        private readonly CategoryAPIService _categoryAPIService;

        public ProductsController(ProductAPIService productAPIService, CategoryAPIService categoryAPIService)
        {
            _productAPIService = productAPIService;
            _categoryAPIService = categoryAPIService;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? keyword, int? categoryID, string? priceRange, int? page,
            int? pageSize = 8)
        {
            var products = await _productAPIService.FilterAsync(keyword, categoryID, priceRange, page, pageSize);

            return View(products);
        }

        // GET: Products/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .Include(b => b.Category)
        //        .Include(b => b.ProductOptions)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    var productOption = _context.ProductOptions.Include(b => b.Product).FirstOrDefault(b => b.ProductId == id);
        //    if (productOption == null)
        //    {
        //        return NotFound();
        //    }

        //    var userID = HttpContext!.Session.GetString("UserId");
        //    ViewBag.userID = userID;

        //    ViewData["Quantity"] = productOption.Quantity;
        //    ViewData["Price"] = productOption.Price;
        //    return View(product);
        //}

        // GET: Products/Create
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
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        //    return View(product);
        //}

        //// POST: Products/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Image,Description,ReleaseDate,CategoryId,IsDeleted")] Product product)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    //if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(product);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ProductExists(product.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
        //    return View(product);
        //}

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
