using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using SunStore.Services;
using SunStore.ViewModel.RequestModels;

namespace SunStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly IVnPayService _vnPayService;

        public CheckoutController(SunStoreContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }
        public IActionResult Index()
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
            var user = _context.Users.Find(uid);
            ViewBag.FullName = user.FullName;
            ViewBag.Email = user.Email;
            ViewBag.Phone = user.PhoneNumber;
            ViewBag.Address = user.Address;
            ViewData["Product"] = _context.Products.ToList();
            return View(Order);
        }

        public List<OrderItem> Order
        {
            get
            {
                var userId = HttpContext!.Session.GetString("UserId");
                int uid = 0;
                if (userId != null)
                {
                    uid = int.Parse(userId);
                }

                var data = _context.OrderItems.Include(o => o.ProductOption)
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

        [HttpPost]
        public IActionResult CreateBill(string address, string number, string note, string voucher, string payment)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
            var user = _context.Users.FirstOrDefault(o => o.Id == uid);

            string tempstr = "" + address[0];
            for (int i = 1; i < address.Length; i++)
            {
                if (address[i] == ' ' && tempstr[tempstr.Length - 1] == ' ') { }
                else
                {
                    tempstr += address[i];
                }
            }
            address = tempstr.Trim();

            var vch = _context.Vouchers.FirstOrDefault(v => v.Code == voucher);
            int? vid = null;
            if (vch != null)
            {
                vid = vch.VoucherId;
            }

            var oid = new Random().Next(100000, 999999);
            while (_context.Orders.FirstOrDefault(o => o.Id == oid) != null) { oid = new Random().Next(100000, 999999); }

            double? total = 0;
            foreach (var item in Order)
            {
                var temp = _context.OrderItems.FirstOrDefault(o => o.Id == item.Id);
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                total += temp.Price;
            }
            total += 20000;
            double? transTotal = total;
            if (vch != null)
            {
                transTotal = total - (total * vch.Vpercent / 100);
            }

            if (payment == "Thanh toán VNPAY")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = transTotal ?? 0,
                    CreatedDate = DateTime.Now,
                    Description = $"{address}_{number}",
                    FullName = user == null ? "" : user.FullName,
                    OrderId = oid,
                };
                HttpContext.Session.SetString("Address", address);
                HttpContext.Session.SetString("Number", number);
                HttpContext.Session.SetString("VoucherId", vid == null ? "" : vid.ToString());
                HttpContext.Session.SetString("Note", note ?? "");
                HttpContext.Session.SetString("OrderId", oid.ToString());
                var vnPayUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);
                Console.WriteLine(vnPayUrl); // Log URL to check
                return Redirect(vnPayUrl);
            }
            var bill = new Order()
            {
                DateTime = DateTime.Now,
                AdrDelivery = address,
                PhoneNumber = number,
                VoucherId = vid,
                Status = "Đã đặt hàng",
                Payment = "COD",
                Note = note
            };
            _context.Orders.Add(bill);
            _context.SaveChanges();

            foreach (var item in Order)
            {
                var temp = _context.OrderItems.FirstOrDefault(o => o.Id == item.Id);
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                product.Quantity -= item.Quantity;
                temp.OrderId = bill.Id;
                _context.OrderItems.Update(temp);
                _context.SaveChanges();
                _context.ProductOptions.Update(product);
                _context.SaveChanges();
            }
            bill.TotalPrice = total;
            if (vch != null && vch.Quantity > 0)
            {
                bill.TotalPrice -= (bill.TotalPrice * vch.Vpercent / 100);
                vch.Quantity--;
                _context.Vouchers.Update(vch);
                _context.SaveChanges();
            }
            _context.Orders.Update(bill);
            _context.SaveChanges();

            //HttpContext.Session.Clear();

            return Redirect("/Home");

        }

        public JsonResult UseVoucher(string code)
        {
            var userId = HttpContext!.Session.GetString("UserId");
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
            var voucher = _context.Vouchers.FirstOrDefault(o => o.Code == code);

            Boolean inuse = false;
            Boolean exist = false;
            Boolean remain = false;
            var percent = 0;
            if (voucher != null)
            {
                exist = true;
                percent = voucher.Vpercent;
                DateTime currentDate = DateTime.Now;
                if (voucher.Quantity > 0 && voucher.EndDate >= currentDate && voucher.StartDate <= currentDate)
                {
                    remain = true;
                }

                var odusevoucher = _context.Orders.Where(o => o.VoucherId == voucher.VoucherId).ToList();
                foreach (var o in odusevoucher)
                {
                    var oid = o.Id;
                    var check = _context.OrderItems.FirstOrDefault(i => i.OrderId == oid && i.CustomerId == uid);
                    if (check != null)
                    {
                        inuse = true;
                    }
                }
                if ((voucher.UserId != null && voucher.UserId != uid))
                {
                    inuse = true;
                }
            }

            return Json(new
            {
                exist,
                remain,
                inuse,
                percent
            });
        }

        public IActionResult PaymentFail() { return View(); }

        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExcute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("Index");
            }

            //Update Database
            var address = HttpContext.Session.GetString("Address");
            var number = HttpContext.Session.GetString("Number");
            var voucher = HttpContext.Session.GetString("VoucherId");
            var note = HttpContext.Session.GetString("Note");
            var oid = HttpContext.Session.GetString("OrderId");
            int? vid = null;
            if (!string.IsNullOrEmpty(voucher))
            {
                vid = int.Parse(voucher);
            }
            var vch = _context.Vouchers.FirstOrDefault(v => v.VoucherId == vid);
            if (string.IsNullOrEmpty(note))
            {
                note = null;
            }


            var bill = new Order()
            {
                DateTime = DateTime.Now,
                AdrDelivery = address,
                PhoneNumber = number,
                VoucherId = vid,
                Status = "Đã đặt hàng",
                Payment = "VNP",
                Note = note
            };
            _context.Orders.Add(bill);
            _context.SaveChanges();

            double? total = 0;
            foreach (var item in Order)
            {
                var temp = _context.OrderItems.FirstOrDefault(o => o.Id == item.Id);
                var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
                product.Quantity -= item.Quantity;
                temp.OrderId = bill.Id;
                total += temp.Price;
                _context.OrderItems.Update(temp);
                _context.SaveChanges();
                _context.ProductOptions.Update(product);
                _context.SaveChanges();
            }
            bill.TotalPrice = total + 20000;
            if (vch != null && vch.Quantity > 0)
            {
                bill.TotalPrice -= (bill.TotalPrice * vch.Vpercent / 100);
                vch.Quantity--;
                _context.Vouchers.Update(vch);
                _context.SaveChanges();
            }
            _context.Orders.Update(bill);
            _context.SaveChanges();

            var userId = HttpContext!.Session.GetString("UserId");

            //HttpContext.Session.Clear();

            HttpContext!.Session.SetString("UserId", userId);
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }
            var user = _context.Users.FirstOrDefault(o => o.Id == uid);

            TempData["Message"] = $"Thanh toán VN Pay thành công!";
            return RedirectToAction("Index");
        }
    }
}
