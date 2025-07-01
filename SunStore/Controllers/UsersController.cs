using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using SunStore.APIServices;
using SunStore.ViewModel.RequestModels;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.Queries;

namespace SunStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly SunStoreContext _context;
        private readonly AuthAPIService _authAPIService;
        private readonly UserAPIService _userAPIService;
        private const string JWT_COOKIE_NAME = "jwtToken";

        public UsersController(SunStoreContext context, AuthAPIService authAPIService, UserAPIService userAPIService)
        {
            _context = context;
            _authAPIService = authAPIService;
            _userAPIService = userAPIService;
        }

        // GET: Users
        public async Task<IActionResult> Index(UserQueryObject userQueryObject)
        {
            var data = await _userAPIService.GetPagedUserAsync(userQueryObject);

            return View(data?.Data);
        }

        // GET: Users/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _authAPIService.GetProfileInfoAsync();

            var user = result?.Data;

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userAPIService.CreateUserAsync(model);

                if (result!.IsSuccessful)
                {
                    TempData["success"] = result.Message;
                }

                else
                {
                    TempData["error"] = result.Message;
                    return View(model);
                }

                return RedirectToAction(nameof(Create));
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userAPIService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateUserRequestViewModel
            {
                Id = user.Data!.Id,
                Address = user.Data.Address,
                BirthDate = user.Data.BirthDate,
                Email = user.Data.Email,
                FullName = user.Data.FullName,
                IsBanned = user.Data.IsBanned,
                PhoneNumber = user.Data.PhoneNumber,
                Username = user.Data.Username,
                Role = user.Data.Role,
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Profile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _authAPIService.GetProfileInfoAsync();

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateUserRequestViewModel
            {
                Id = user.Data!.Id,
                Address = user.Data.Address,
                BirthDate = user.Data.BirthDate,
                Email = user.Data.Email,
                FullName = user.Data.FullName,
                IsBanned = user.Data.IsBanned,
                PhoneNumber = user.Data.PhoneNumber,
                Username = user.Data.Username,
                Role = user.Data.Role,
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(int? id, UpdateUserRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userAPIService.UpdateUserAsync(model);

                if (result.IsSuccessful)
                {
                    TempData["success"] = result.Message;
                }

                else
                {
                    TempData["error"] = result.Message;
                    return View(model);
                }

                return RedirectToAction(nameof(Profile));
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, UpdateUserRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userAPIService.UpdateUserAsync(model);

                if (result!.IsSuccessful)
                {
                    TempData["success"] = result.Message;
                }

                else
                {
                    TempData["error"] = result.Message;
                    return View(model);
                }

                return RedirectToAction(nameof(Edit), new { id });
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public IActionResult Login()
        {
            ViewBag.ResetPassword = TempData["ResetPassword"];
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var result = await _authAPIService.SignOutAsync();

            if (result)
            {
                Response.Cookies.Delete(JWT_COOKIE_NAME);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authAPIService.LoginAsync(model);

                if (result.IsSuccessful == false)
                {
                    ViewBag.Error = result.ErrorMessage;
                    return View();
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddHours(5)
                };

                // Append Jwt token to the cookie.
                Response.Cookies.Append(JWT_COOKIE_NAME, result.Token!, cookieOptions);

                #region temporary code

                var user = _context.Users
                    .Where(e => e.Email == model.Email && e.Password == model.Password)
                    .FirstOrDefault();

                // Store account info in session or cookie
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString()!);
                HttpContext.Session.SetString("UserName", user.FullName!.ToString()!);
                HttpContext.Session.SetString("UserEmail", user.Email!.ToString()!);

                int uid = user.Id;

                var cartQuantity = _context.OrderItems.Where(o => o.OrderId == 0 && o.CustomerId == uid).Count();
                HttpContext.Session.SetString("CartQuantity", cartQuantity.ToString());

                #endregion

                return RedirectToAction("Index", "Home");
            }

            catch (HttpRequestException ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        public IActionResult NewPassword(string? email, string? otp)
        {
            var modelData = new UpdatePasswordRequestViewModel { Email = email, Otp = otp };

            // Prevent access to the page if the email or the otp code is not present in the query string.
            if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(NotFoundPage));
            }

            return View(modelData);
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(UpdatePasswordRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authAPIService.UpdatePasswordAsync(model);

                if (result!.IsSuccessful)
                {
                    TempData["ResetPassword"] = result.Message;
                    return RedirectToAction(nameof(Login));
                }

                ViewBag.Error = result.Message;
                return View(model);
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult VerifyOTP(string email)
        {
            var modelData = new OTPVerificationRequestViewModel { Email = email };
            return View(modelData);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(OTPVerificationRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authAPIService.SendOTPVerificationRequestAsync(model);

                if (result!.IsSuccessful)
                {
                    return RedirectToAction(nameof(NewPassword), new { email = model.Email, otp = model.Otp });
                }

                ViewBag.Error = result.Message;
                return View(model);
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authAPIService.SendResetPasswordRequestAsync(model);

                if (result!.IsSuccessful)
                {
                    return RedirectToAction(nameof(VerifyOTP), new { email = model.Email });
                }

                ViewBag.Error = result.Message;
                return View(model);
            }

            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult RegisterSuccessful()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authAPIService.RegisterAsync(model);

                if (result!.IsSuccessful)
                {
                    return RedirectToAction(nameof(RegisterSuccessful));
                }

                ViewBag.Error = result.Message;
                return View(model);
            }

            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                return View(model);
            }
        }
    }
}
