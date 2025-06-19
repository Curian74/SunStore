using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Security.Claims;

namespace SunStore.Controllers
{
    public class CartController : Controller
    {
        private readonly SunStoreContext _context;

        public CartController(SunStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Product"] = _context.Products.ToList();
            return View(Orders);
        }

        public List<OrderItem> Orders
        {
            get
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
                int uid = 0;
                if (userId != null)
                {
                    uid = int.Parse(userId);
                }

                var data = _context.OrderItems.Include(o => o.Product)
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
        public JsonResult AddToCart(int id, int quantity)
        {
            var myCart = Orders;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
            var item = myCart.FirstOrDefault(c => c.ProductId == id);
            var product = _context.ProductOptions.Include(b => b.Product).FirstOrDefault(b => b.Id == id);
            var exist = false;
            if (item == null)
            {
                item = new OrderItem()
                {
                    ProductId = product.Id,
                    CustomerId = userId == null ? 0 : int.Parse(userId),
                    OrderId = 0,
                    Quantity = quantity,
                    Price = product.Price * quantity
                };
                _context.OrderItems.Add(item);
                _context.SaveChanges();
            }
            else
            {
                exist = true;
                var orderItem = _context.OrderItems.Find(item.Id);
                orderItem.Quantity = quantity;
                orderItem.Price = product.Price * quantity;
                _context.OrderItems.Update(orderItem);
                _context.SaveChanges();
            }

            int uid = 0;
            if (userId != null)
            {
                uid = Int32.Parse(userId);
            }

            var cartQuantity = _context.OrderItems.Where(o => o.OrderId == 0 && o.CustomerId == uid).Count();
            HttpContext.Session.SetString("CartQuantity", cartQuantity.ToString());

            return Json(new
            {
                exist
            });
        }

        public JsonResult DeleteItem(int id)
        {
            var item = _context.OrderItems.Find(id);

            if (item != null)
            {
                _context.OrderItems.Remove(item);
                _context.SaveChanges();
            }

            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = Int32.Parse(userId);
            }

            var cartQuantity = _context.OrderItems.Where(o => o.OrderId == 0 && o.CustomerId == uid).Count();
            HttpContext.Session.SetString("CartQuantity", cartQuantity.ToString());

            var items = _context.OrderItems.Where(o => o.CustomerId == uid && o.OrderId == 0);
            var total = items.Sum(o => o.Price);

            return Json(new
            {
                total
            });
        }

        public JsonResult IncOne(int id)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = Int32.Parse(userId);
            }
            var item = _context.OrderItems.Find(id);
            var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);

            var unitprice = product.Price;
            var MaxQuantity = product.Quantity;
            if (item != null && item.Quantity < MaxQuantity)
            {
                item.Quantity++;
                item.Price = unitprice * item.Quantity;
                _context.OrderItems.Update(item);
                _context.SaveChanges();
            }

            var items = _context.OrderItems.Where(o => o.CustomerId == uid && o.OrderId == 0);
            var total = items.Sum(o => o.Price);

            return Json(new
            {
                quantity = item.Quantity,
                price = item.Price,
                unitprice,
                total
            });
        }

        public JsonResult DecOne(int id)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = Int32.Parse(userId);
            }
            var item = _context.OrderItems.Find(id);
            var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);

            var unitprice = product.Price;
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                item.Price = unitprice * item.Quantity;
                _context.OrderItems.Update(item);
                _context.SaveChanges();
            }

            var items = _context.OrderItems.Where(o => o.CustomerId == uid && o.OrderId == 0);
            var total = items.Sum(o => o.Price);

            return Json(new
            {
                quantity = item.Quantity,
                price = item.Price,
                unitprice,
                total
            });
        }

        public JsonResult UpdateQuantity(int id, int quantity)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = Int32.Parse(userId);
            }
            var item = _context.OrderItems.Find(id);
            var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);

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
            var total = items.Sum(o => o.Price);

            return Json(new
            {
                quantity = item.Quantity,
                price = item.Price,
                unitprice,
                total
            });
        }
    }
}
