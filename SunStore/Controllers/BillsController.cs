using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace SunStore.Controllers
{
    public class BillsController : Controller
    {
        private readonly SunStoreContext _context;

        public BillsController(SunStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page, DateTime? fromDate, DateTime? toDate)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }

            var orders = new List<Order>();
            foreach (var order in _context.Orders.ToList())
            {
                var item = _context.OrderItems.FirstOrDefault(o => o.CustomerId == uid && o.OrderId == order.Id);
                if (item != null)
                {
                    orders.Add(order);
                }
            }

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
            
            //Pagination
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            var od = orders.OrderByDescending(o => o.DateTime).ToPagedList(pageNumber, pageSize);
            return View(od);
        }

        public IActionResult Detail(int id)
        {
            var items = _context.OrderItems.Include(o => o.ProductOption).Where(o => o.OrderId == id).ToList();
            var order = _context.Orders.Find(id);
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
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

            ViewBag.Shipper = shipperName;
            ViewBag.Voucher = code;
            ViewData["Product"] = _context.Products.ToList();
            return View(items);
        }

        public IActionResult Delete(int id)
        {
            var items = _context.OrderItems.Include(o => o.ProductOption).Where(o => o.OrderId == id).ToList();
            var order = _context.Orders.Find(id);
            foreach (var item in items)
            {
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                product.Quantity += item.Quantity;
                _context.ProductOptions.Update(product);
                _context.SaveChanges();
            }
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Cancel(int id)
        {
            var items = _context.OrderItems.Include(o => o.ProductOption).Where(o => o.OrderId == id).ToList();
            var order = _context.Orders.Find(id);
            foreach (var item in items)
            {
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                product.Quantity += item.Quantity;
                _context.ProductOptions.Update(product);
                _context.SaveChanges();
            }
            order.Status = "Đã huỷ";
            _context.Orders.Update(order);
            _context.SaveChanges();
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
    }
}
