using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;

namespace SunStore.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly SunStoreContext _context;

        public OrderItemsController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            var sunStoreContext = _context.OrderItems.Include(o => o.Customer).Include(o => o.Order).Include(o => o.Product);
            return View(await sunStoreContext.ToListAsync());
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Customer)
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "UserId", "UserId");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.ProductOptions, "Id", "Id");
            return View();
        }

        // POST: OrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,CustomerId,OrderId,Quantity,Price")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "UserId", "UserId", orderItem.CustomerId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.ProductOptions, "Id", "Id", orderItem.ProductId);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "UserId", "UserId", orderItem.CustomerId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.ProductOptions, "Id", "Id", orderItem.ProductId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,CustomerId,OrderId,Quantity,Price")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "UserId", "UserId", orderItem.CustomerId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.ProductOptions, "Id", "Id", orderItem.ProductId);
            return View(orderItem);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Customer)
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
