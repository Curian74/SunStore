using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using BusinessObjects;
using SunStoreAPI.Dtos;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public ProductsController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        //GET: api/Products/filter
        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts(
        string? keyword,
        int? categoryID,
        string? priceRange,
        int? page = 1,
        int pageSize = 9)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
                if (keyword.Length > 100)
                {
                    return BadRequest("Từ khóa tìm kiếm không được vượt quá 100 ký tự.");
                }
            }

            var productQuery = _context.Products
                .Include(b => b.Category)
                .Include(b => b.ProductOptions)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                productQuery = productQuery.Where(p =>
        p.Name != null && p.Name.ToLower().Contains(keyword.ToLower()));
            }

            if (categoryID.HasValue)
            {
                productQuery = productQuery.Where(b => b.CategoryId == categoryID.Value);
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceParts = priceRange.Split('-');
                if (priceParts.Length == 2 &&
                    Decimal.TryParse(priceParts[0], out decimal minPrice) &&
                    Decimal.TryParse(priceParts[1], out decimal maxPrice))
                {
                    productQuery = productQuery.Where(b =>
                        b.ProductOptions.Any(bo => (decimal)bo.Price >= minPrice && (decimal)bo.Price <= maxPrice));
                }
                else if (priceRange.EndsWith("+") &&
                         Decimal.TryParse(priceRange.TrimEnd('+'), out decimal minPriceOnly))
                {
                    productQuery = productQuery.Where(b =>
                        b.ProductOptions.Any(bo => (decimal)bo.Price >= minPriceOnly));
                }
            }

            var totalItems = await productQuery.CountAsync();
            var products = await productQuery
                .Skip((page.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<ProductListResponseDto>
            {
                CurrentPage = (int) page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = products.Select(p => new ProductListResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image,
                    Category = p.Category == null ? null : new CategoryResponseModel
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    },
                    ProductOptions = p.ProductOptions.Select(po => new ProductOptionResponseModel
                    {
                        Id = po.Id,
                        Size = po.Size,
                        Price = po.Price
                    }).ToList()
                }).ToList()
            };

            return Ok(result);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
