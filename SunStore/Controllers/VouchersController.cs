using BusinessObjects.Constants;
using BusinessObjects.Models;
using BusinessObjects.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SunStore.APIServices;
using SunStore.ViewModel.DataModels;
using SunStore.ViewModel.RequestModels;
using System.Threading.Tasks;

namespace SunStore.Controllers
{
    public class VouchersController : Controller
    {
        private readonly VoucherAPIService _voucherService;
        private readonly UserAPIService _userAPIService;
        private readonly SunStoreContext _context;

        public VouchersController(VoucherAPIService voucherService, UserAPIService userAPIService,
            SunStoreContext context)
        {
            _voucherService = voucherService;
            _userAPIService = userAPIService;
            _context = context;
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
                Role = int.Parse(UserRoleConstants.Customer),
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
                    var message = response!.Message;

                    if (response!.IsSuccessful)
                    {
                        TempData["voucherSuccess"] = message;
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, message!);
                }

                var queryObjectMock = new UserQueryObject
                {
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                    Role = int.Parse(UserRoleConstants.Customer),
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
                    Role = int.Parse(UserRoleConstants.Customer),
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

            var queryObjectMock = new UserQueryObject
            {
                CurrentPage = 1,
                PageSize = int.MaxValue,
                Role = int.Parse(UserRoleConstants.Customer),
            };

            var voucherCustomer = _context.VoucherCustomers
                .Where(x => x.VoucherId == voucher.VoucherId)
                .Select(x => x.CustomerId!.Value)
                .ToList();

            var users = await _userAPIService.GetPagedUserAsync(queryObjectMock);

            var vm = new EditVoucherViewModel
            {
                Users = users!.Data!.Items.Select(x => new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Id.ToString(),
                })
                .ToList(),
                UserIds = voucherCustomer,
                Code = voucher.Code,
                EndDate = voucher.EndDate,
                StartDate = voucher.StartDate,
                Quantity = voucher.Quantity,
                Vpercent = voucher.Vpercent,
                VoucherId = voucher.VoucherId,
            };

            return View(vm);
        }

        // POST: Vouchers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditVoucherViewModel model)
        {
            var voucher = await _voucherService.GetByIdAsync(id);

            if (voucher == null)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _voucherService.UpdateAsync(model);
                    var message = response!.Message;

                    if (response!.IsSuccessful)
                    {
                        TempData["voucherSuccess"] = message;
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, message!);
                }

                var queryObjectMock = new UserQueryObject
                {
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                    Role = int.Parse(UserRoleConstants.Customer),
                };

                var users = await _userAPIService.GetPagedUserAsync(queryObjectMock);

                model.Users = users!.Data!.Items.Select(x => new SelectListItem
                {
                    Text = x.Username,
                    Value = x.Id.ToString(),
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không xác định khi cập nhật voucher." + ex.Message);

                var queryObjectMock = new UserQueryObject
                {
                    CurrentPage = 1,
                    PageSize = int.MaxValue,
                    Role = int.Parse(UserRoleConstants.Customer),
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

