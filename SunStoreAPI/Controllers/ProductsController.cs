using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using BusinessObjects;
using SunStoreAPI.Dtos.Requests;
using SunStoreAPI.Services;
using BusinessObjects.ApiResponses;
using CategoryResponseModel = SunStoreAPI.Dtos.Requests.CategoryResponseModel;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SunStoreContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(SunStoreContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                CurrentPage = (int)page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = products.Select(p => new ProductListResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image,
                    ReleaseDate = p.ReleaseDate,
                    IsDeleted = p.IsDeleted,
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
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] CreateProductRequestDto dto)
        {
            try
            {
                var newProduct = new Product
                {
                    CategoryId = dto.CategoryId,
                    Description = dto.Description,
                    IsDeleted = dto.IsDeleted,
                    Name = dto.Name,
                    ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
                    Image = dto.ImageUrl,
                };

                await _context.Products.AddAsync(newProduct);
                await _context.SaveChangesAsync();

                return Ok(new BaseApiResponse
                {
                    Message = "Thành công.",
                    IsSuccessful = true,
                });
            }

            catch (Exception ex)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var imageHandlerService = new ImageHandlerService(_env);

                var url = await imageHandlerService.HandleImageUpload(file, "ProductImg");

                return Ok(new ApiResult<string>
                {
                    IsSuccessful = true,
                    Data = url,
                });
            }

            catch (Exception ex)
            {
                return BadRequest(new ApiResult<string>
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }
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
