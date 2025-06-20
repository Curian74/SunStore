using BusinessObjects.ApiResponses;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using BusinessObjects.RequestModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly SunStoreContext _context;
        public CartsController(SunStoreContext context)
        {
            _context = context;
        }

        public List<OrderItem> listCart
        {
            get
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
                int uid = 0;
                if (userId != null)
                {
                    uid = int.Parse(userId);
                }

                var data = _context.OrderItems.Include(o => o.ProductOption)
                                                .ThenInclude(p => p.Product)
                                              .Where(o => o.CustomerId == uid)
                                              .Where(o => o.OrderId == 0)
                                              .ToList();
                if (data == null)
                {
                    data = new List<OrderItem>();
                }
                return data;
            }
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCart(int? customerId)
        {
            var cartItems = await _context.OrderItems
                .Include(x => x.ProductOption)
                .ThenInclude(p => p.Product)
                .Where(x => x.CustomerId == customerId && x.OrderId == 0)
                .Select(x => new CartItemDto
                {
                    Id = x.Id,
                    ProductOptionId = x.ProductId,
                    ProductName = x.ProductOption.Product.Name,
                    ProductImage = x.ProductOption.Product.Image,
                    Size = x.ProductOption.Size,
                    Price = x.ProductOption.Price,
                    TotalQuantity = x.ProductOption.Quantity,
                    Quantity = x.Quantity
                }).ToListAsync();

            return Ok(cartItems);
        }

        [HttpPost("add")]
        public ApiResult AddToCart([FromBody] CartActionRequest model)
        {
            var myCart = listCart;
            var userId = model.CustomerId;
            var item = myCart.FirstOrDefault(c => c.ProductId == model.ProductOptionId);
            var product = _context.ProductOptions.Include(b => b.Product).FirstOrDefault(b => b.Id == model.ProductOptionId);
            var exist = false;
            if (item == null)
            {
                item = new OrderItem()
                {
                    ProductId = product!.Id,
                    CustomerId = userId ?? 0,
                    OrderId = 0,
                    Quantity = model.Quantity,
                    Price = product.Price * model.Quantity,
                };
                _context.OrderItems.Add(item);
                _context.SaveChanges();
            }
            else
            {
                exist = true;
                var orderItem = _context.OrderItems.Find(item.Id);
                orderItem.Quantity = model.Quantity;
                orderItem.Price = product!.Price * model.Quantity;
                _context.OrderItems.Update(orderItem);
                _context.SaveChanges();
            }

            //var cartQuantity = _context.OrderItems.Where(o => o.OrderId == 0 && o.CustomerId == uid).Count();
            //HttpContext.Session.SetString("CartQuantity", cartQuantity.ToString());

            return new ApiResult
            {
                IsSuccessful = true,
                Message = exist.ToString(),
            };
        }

        [HttpPost("update-quantity")]
        public async Task<ActionResult<CartInfoResponse>> UpdateQuantity([FromBody] UpdateQuantityRequest model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
            int? uid = null;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
            var item = _context.OrderItems.Find(model.CartItemId);
            var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
            var quantity = model.Quantity; 

            var unitprice = product.Price;
            var MaxQuantity = product.Quantity;
            if (item != null)
            {
                if (quantity > MaxQuantity)
                {
                    item.Quantity = MaxQuantity;
                }
                else
                {
                    item.Quantity = quantity;
                }
                item.Price = unitprice * item.Quantity;
                _context.OrderItems.Update(item);
                _context.SaveChanges();
            }

            var items = _context.OrderItems.Where(o => o.CustomerId == uid && o.OrderId == 0);
            var total = items.Sum(o => o.Price ?? 0);

            var result = new CartInfoResponse
            {
                Quantity = item.Quantity,
                Price = item.Price,
                UnitPrice = unitprice,
                Total = total,
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null)
                return NotFound(new ApiResult { IsSuccessful = false, Message = "Cart item not found." });

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int uid = userId != null ? int.Parse(userId) : 0;
            var total = await _context.OrderItems
                .Where(o => o.CustomerId == uid && o.OrderId == 0)
                .SumAsync(o => o.Price);

            var response = new CartInfoResponse
            {
                Total = total
            };

            return Ok(response);
        }

    }
}
