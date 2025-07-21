using Microsoft.AspNetCore.Mvc;
using BusinessObjects.Models;
using SunStore.APIServices;
using System.Threading.Tasks;
using BusinessObjects.Queries;
using SunStore.ViewModel.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SunStore.Controllers
{
    public class VouchersController : Controller
    {
        private readonly VoucherAPIService _voucherService;
        private readonly UserAPIService _userAPIService;

        public VouchersController(VoucherAPIService voucherService, UserAPIService userAPIService)
        {
            _voucherService = voucherService;
            _userAPIService = userAPIService;
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
        public async Task<IActionResult> Create()
        {
            var queryObjectMock = new UserQueryObject
            {
                CurrentPage = 1,
                PageSize = int.MaxValue,
            };

            var users = await _userAPIService.GetPagedUserAsync(queryObjectMock);

            var vm = new CreateVoucherViewModel
            {
                Users = users!.Data!.Items.Select(x => new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Id.ToString(),
                })
                .ToList(),
            };

            return View(vm);
        }

        // POST: Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _voucherService.CreateAsync(model);
                    if (response!.IsSuccessful)
                        return RedirectToAction(nameof(Index));

                    var message = response.Message;

                    ModelState.AddModelError(string.Empty, message!);
                }

                var queryObjectMock = new UserQueryObject
                {
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                };

                var users = await _userAPIService.GetPagedUserAsync(queryObjectMock);

                model.Users = users!.Data!.Items.Select(x => new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Id.ToString(),
                }).ToList();

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không xác định khi tạo voucher.");

                var queryObjectMock = new UserQueryObject
                {
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                };

                var users = await _userAPIService.GetPagedUserAsync(queryObjectMock);

                model.Users = users!.Data!.Items.Select(x => new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Id.ToString(),
                }).ToList();

                return View(model);
            }
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

