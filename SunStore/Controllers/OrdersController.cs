using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using X.PagedList.Extensions;

namespace SunStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly SunStoreContext _context;

        public OrdersController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: Orders
        //public async Task<IActionResult> Index()
        //{
        //    var sunStoreContext = _context.Orders.Include(o => o.Shipper).Include(o => o.Voucher);
        //    return View(await sunStoreContext.ToListAsync());
        //}

        public IActionResult Index(int? page, DateTime? fromDate, DateTime? toDate)
        {
            var orders = _context.Orders.ToList();

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.DateTime >= fromDate.Value).ToList();
                ViewBag.From = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.DateTime <= toDate.Value.AddDays(1)).ToList();
                ViewBag.To = toDate.Value.ToString("yyyy-MM-dd");
            }

            var sum = orders.Where(o => o.Status == "Đã giao hàng").Sum(o => o.TotalPrice);
            var count = orders.Where(o => o.Status == "Đã giao hàng").ToList().Count();
            var today = DateTime.Now.ToShortDateString();

            var temp = _context.Orders.ToList();
            var rtd = temp.Where(o => {
                var day = o.DateTime.Value.ToShortDateString();
                return (day == today) && (o.Status == "Đã giao hàng");
            })
                            .Sum(o => o.TotalPrice);

            int pageSize = 6;
            int pageNumber = (page ?? 1);

            var od = orders.OrderByDescending(o => o.DateTime).ToPagedList(pageNumber, pageSize);

            ViewBag.Revenue = sum;
            ViewBag.NumberOrders = count;
            ViewBag.RToday = rtd;
            return View(od);
        }

        public IActionResult Details(int id)
        {
            var items = _context.OrderItems.Include(o => o.Product).Where(o => o.OrderId == id).ToList();
            var order = _context.Orders.Find(id);

            var uid = items.FirstOrDefault().CustomerId;

            var user = _context.Users.Find(uid);
            ViewBag.FullName = user.FullName;
            ViewBag.Phone = order.PhoneNumber;
            ViewBag.Address = order.AdrDelivery;
            ViewBag.Status = order.Status;
            ViewBag.Note = order.Note;
            ViewBag.Payment = order.Payment;

            var voucherPercent = 0;
            if (order.VoucherId != null)
            {
                voucherPercent = _context.Vouchers.FirstOrDefault(v => v.VoucherId == order.VoucherId).Vpercent;
            }
            ViewBag.VoucherPercent = voucherPercent;

            var shipper = _context.Users.FirstOrDefault(u => u.Id == order.ShipperId);
            var voucher = _context.Vouchers.FirstOrDefault(v => v.VoucherId == order.VoucherId);
            string staffName = "Chưa có", shipperName = "Chưa có", code = "Không có";
            if (shipper != null)
            {
                shipperName = shipper.FullName;
            }
            if (voucher != null && voucher.Code != null)
            {
                code = voucher.Code;
            }

            ViewBag.Staff = staffName;
            ViewBag.Shipper = shipperName;
            ViewBag.Voucher = code;
            ViewData["Product"] = _context.Products.ToList();
            return View(items);
        }

        public IActionResult Cancel(int id, string reason)
        {
            var items = _context.OrderItems.Include(o => o.Product).Where(o => o.OrderId == id).ToList();

            var order = _context.Orders.Find(id);
            foreach (var item in items)
            {
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                product.Quantity += item.Quantity;
                _context.ProductOptions.Update(product);
                _context.SaveChanges();
            }

            if (reason == "default") reason = "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm.";

            order.Status = "Bị từ chối";
            order.DenyReason = reason;
            _context.Orders.Update(order);
            _context.SaveChanges();

            var cusId = _context.OrderItems.FirstOrDefault(o => o.OrderId == id).CustomerId;
            var email = _context.Users.FirstOrDefault(u => u.Id == cusId).Email;

            return RedirectToAction("Index");
        }

        public JsonResult GetCancelReason(int id)
        {
            var mess = _context.Orders.FirstOrDefault(o => o.Id == id);
            var reason = "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm.";
            if (mess != null)
            {
                if (mess.DenyReason != null)
                {
                    reason = mess.DenyReason;
                }
            }
            return Json(new
            {
                reason
            });
        }

        //// GET: Orders/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var order = await _context.Orders
        //        .Include(o => o.Shipper)
        //        .Include(o => o.Voucher)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(order);
        //}

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["ShipperId"] = new SelectList(_context.Employees, "UserId", "UserId");
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "VoucherId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ShipperId,VoucherId,DateTime,AdrDelivery,PhoneNumber,Note,TotalPrice,Payment,Status,DenyReason")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ShipperId"] = new SelectList(_context.Employees, "UserId", "UserId", order.ShipperId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "VoucherId", order.VoucherId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["ShipperId"] = new SelectList(_context.Employees, "UserId", "UserId", order.ShipperId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "VoucherId", order.VoucherId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ShipperId,VoucherId,DateTime,AdrDelivery,PhoneNumber,Note,TotalPrice,Payment,Status,DenyReason")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["ShipperId"] = new SelectList(_context.Employees, "UserId", "UserId", order.ShipperId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "VoucherId", "VoucherId", order.VoucherId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Shipper)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
