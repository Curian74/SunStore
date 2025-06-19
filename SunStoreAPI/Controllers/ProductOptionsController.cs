using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using BusinessObjects.ApiResponses;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOptionsController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public ProductOptionsController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: api/ProductOptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductOption>>> GetProductOptions()
        {
            return await _context.ProductOptions.ToListAsync();
        }

        // GET: api/ProductOptions/detail/5
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<ProductDetailResponse>> GetProductOptionDetail(int id)
        {
            var option = await _context.ProductOptions.Include(po => po.Product)
                                                          .ThenInclude(p => p.Category)
                                                      .Include(po => po.Product.ProductOptions)
                                                      .FirstOrDefaultAsync(po => po.Id == id);

            if (option == null) return NotFound();

            var product = option.Product;

            var result = new ProductDetailResponse
            {
                // Product info
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImage = product.Image,
                ProductDescription = product.Description,
                ReleaseDate = product.ReleaseDate,
                Category = new CategoryResponseModel
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                },

                // Option selected
                OptionId = option.Id,
                Size = option.Size,
                Quantity = option.Quantity,
                Price = option.Price,
                Rating = option.Rating,
                Discount = option.Discount,

                // List other options
                OtherOptions = product.ProductOptions
                    .Select(po => new ProductOptionSummaryModel
                    {
                        Id = po.Id,
                        Size = po.Size,
                        Price = po.Price
                    }).ToList()
            };

            return Ok(result);
        }

        // GET: api/ProductOptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductOption>> GetProductOption(int id)
        {
            var productOption = await _context.ProductOptions.FindAsync(id);

            if (productOption == null)
            {
                return NotFound();
            }

            return productOption;
        }

        // PUT: api/ProductOptions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductOption(int id, ProductOption productOption)
        {
            if (id != productOption.Id)
            {
                return BadRequest();
            }

            _context.Entry(productOption).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductOptionExists(id))
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

        // POST: api/ProductOptions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductOption>> PostProductOption(ProductOption productOption)
        {
            _context.ProductOptions.Add(productOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductOption", new { id = productOption.Id }, productOption);
        }

        // DELETE: api/ProductOptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductOption(int id)
        {
            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption == null)
            {
                return NotFound();
            }

            _context.ProductOptions.Remove(productOption);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductOptionExists(int id)
        {
            return _context.ProductOptions.Any(e => e.Id == id);
        }
    }
}
