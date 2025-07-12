using System.Security.Claims;
using BusinessObjects.Constants;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SunStore.APIServices;

namespace SunStore.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly OrderAPIService _orderAPIService;

        public EmployeesController(SunStoreContext context, OrderAPIService orderAPIService)
        {
            _context = context;
            _orderAPIService = orderAPIService;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var sunStoreContext = _context.Employees.Include(e => e.User);
            return View(await sunStoreContext.ToListAsync());
        }

        public async Task<IActionResult> AssignShipper()
        {
            List<Order> orders;

            try
            {
                orders = await _orderAPIService.GetUnassignedOrders();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy danh sách đơn hàng. " + ex.Message);
                orders = [];
            }

            var shippers = _context.Users.Where(o => o.Role == 3).ToList();
            ViewBag.Shippers = shippers;
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int orderId, int shipperId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == null);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int uid = 0;
            if (userId != null)
            {
                uid = int.Parse(userId);
            }

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng hợp lệ để cập nhật.");
            }

            order.ShipperId = shipperId;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        //Shipper parts
        public async Task<IActionResult> Order()
        {
            int shipperId = int.Parse(HttpContext!.Session.GetString("UserId"));
            var orders = await _context.Orders.Where(o => o.ShipperId == shipperId).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false });
            }

            order.Status = OrderStatusConstant.Shipping; // Cập nhật trạng thái từ "Đã đặt hàng" sang "Đang giao hàng"
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            var items = _context.OrderItems.Include(o => o.ProductOption).Where(o => o.OrderId == orderId).ToList();
            if (order == null)
            {
                return Json(new { success = false });
            }
            //foreach (var item in items)
            //{
            //    var product = _context.ProductOptions.FirstOrDefault(b => b.Id == item.ProductId);
            //    product.Quantity += item.Quantity;
            //    _context.ProductOptions.Update(product);
            //    _context.SaveChanges();
            //}
            //order.DenyReason = "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm.";
            order.Status = "Đã đặt hàng";
            order.ShipperId = null;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId); 
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false });
            }

            order.Status = "Đã giao hàng"; // Cập nhật trạng thái từ "Đang giao hàng" sang "Đã giao hàng"
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////
        /// </summary>
   
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Salary,HiredDate,Status")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", employee.UserId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", employee.UserId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Salary,HiredDate,Status")] Employee employee)
        {
            if (id != employee.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.UserId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", employee.UserId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.UserId == id);
        }
    }
}
