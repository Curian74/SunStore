﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using SunStore.ViewModel.RequestModels;
using SunStore.APIServices;

namespace SunStore.Controllers
{
    public class ProductOptionsController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly ProductOptionAPIService _productOptionAPIService;
        private readonly ProductAPIService _productAPIService;

        public ProductOptionsController(SunStoreContext context, ProductOptionAPIService productOptionAPIService, 
            ProductAPIService productAPIService)
        {
            _context = context;
            _productOptionAPIService = productOptionAPIService;
            _productAPIService = productAPIService;
        }

        // GET: ProductOptions
        public async Task<IActionResult> Index()
        {
            var sunStoreContext = _context.ProductOptions.Include(p => p.Product);
            return View(await sunStoreContext.ToListAsync());
        }

        // GET: ProductOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions
                .Include(b => b.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (productOption == null)
            {
                return NotFound();
            }

            var userID = HttpContext!.Session.GetString("UserId");
            ViewBag.userID = userID;

            ViewData["ProductOptions"] = _context.ProductOptions.Include(b => b.Product).Where(bo => bo.ProductId == productOption.ProductId).ToList();
            return View(productOption);
        }

        // GET: ProductOptions/Create
        public async Task<IActionResult> Create()
        {
            var products = await _productAPIService.FilterAsync("", null, null, null, 1000);

            return View(new CreateProductOptionRequestViewModel
            {
                Products = products.Items.Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString(),
                }).ToList(),
            });
        }

        // POST: ProductOptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductOptionRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Create), model);
            }

            try
            {

                var result = await _productOptionAPIService.CreateAsync(model);

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

        // GET: ProductOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productOption.ProductId);
            return View(productOption);
        }

        // POST: ProductOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Size,Quantity,Price,Rating,Discount,ProductId")] ProductOption productOption)
        {
            if (id != productOption.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductOptionExists(productOption.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productOption.ProductId);
            return View(productOption);
        }

        // GET: ProductOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productOption == null)
            {
                return NotFound();
            }

            return View(productOption);
        }

        // POST: ProductOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption != null)
            {
                _context.ProductOptions.Remove(productOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductOptionExists(int id)
        {
            return _context.ProductOptions.Any(e => e.Id == id);
        }
    }
}
