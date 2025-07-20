using Microsoft.AspNetCore.Mvc;
using BusinessObjects.Models;
using SunStore.APIServices;

namespace SunStore.Controllers
{
    public class VouchersController : Controller
    {
        private readonly VoucherAPIService _voucherService;

        public VouchersController(VoucherAPIService voucherService)
        {
            _voucherService = voucherService;
        }

        // GET: Vouchers
        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllAsync();
            return View(vouchers);
        }

        // GET: Vouchers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        // GET: Vouchers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                var response = await _voucherService.CreateAsync(voucher);
                if (response.IsSuccess) return RedirectToAction(nameof(Index));
                string message = "Tạo voucher thất bại.";
                if(response.ErrorMessage == "Voucher code has existed.")
                {
                    message = "Tạo voucher thất bại do Voucher Code đã tồn tại";
                }
                ModelState.AddModelError("", message);
            }
            return View(voucher);
        }

        // GET: Vouchers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        // POST: Vouchers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voucher voucher)
        {
            if (id != voucher.VoucherId) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _voucherService.UpdateAsync(id, voucher);
                if (success) return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "Cập nhật voucher thất bại.");
            }
            return View(voucher);
        }

        // GET: Vouchers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        // POST: Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _voucherService.DeleteAsync(id);
            if (success) return RedirectToAction(nameof(Index));
            return BadRequest("Xoá thất bại.");
        }
    }
}

