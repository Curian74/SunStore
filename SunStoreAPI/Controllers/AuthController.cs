using BusinessObjects.ApiResponses;
using BusinessObjects.Constants;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos;
using SunStoreAPI.Dtos.Requests;
using SunStoreAPI.Dtos.User;
using SunStoreAPI.Services;
using SunStoreAPI.Utils;
using System.Security.Claims;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string JWT_COOKIE_NAME = "jwtToken";
        private const int OTP_EXPIRATION_MINUTES = 10;

        private readonly SunStoreContext _context;
        private readonly JwtTokenProvider _jwtTokenProvider;
        private readonly EmailService _emailService;
        private readonly CacheUtils _cacheUtils;

        public AuthController(SunStoreContext context, JwtTokenProvider jwtTokenProvider,
            EmailService emailService, CacheUtils cacheUtils)
        {
            _context = context;
            _jwtTokenProvider = jwtTokenProvider;
            _emailService = emailService;
            _cacheUtils = cacheUtils;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var loginResponseDto = new LoginResponseDto();

            // 1. Check if user exists.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                loginResponseDto.ErrorMessage = "Không tìm thấy tài khoản.";
                return Unauthorized(loginResponseDto);
            }

            // 2. Check if user is banned.
            if (user.IsBanned == 1)
            {
                loginResponseDto.ErrorMessage = "Tài khoản của bạn đã bị chặn!";
                return Unauthorized(loginResponseDto);
            }

            // 3. Check password.
            //if (dto.Password != user.Password)
            //{
            //    loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            //    return Unauthorized(loginResponseDto);
            //}

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return Unauthorized(loginResponseDto);
            }

            // 4. Create JWT.
            string jwtToken = _jwtTokenProvider.CreateToken(user);

            // 5. Set JWT in cookie.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(5)
            };

            Response.Cookies.Append(JWT_COOKIE_NAME, jwtToken, cookieOptions);

            loginResponseDto.IsSuccessful = true;
            loginResponseDto.Token = jwtToken;

            return Ok(loginResponseDto);
        }

        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(JWT_COOKIE_NAME);

            return Ok("Logged out");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var emailExisted = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email) != null;

            if (emailExisted)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Email này đã tồn tại."
                });
            }

            var phoneExisted = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber != null && u.PhoneNumber == dto.PhoneNumber) != null;

            if (phoneExisted)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Số điện thoại đã tồn tại."
                });
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Mật khẩu không khớp!"
                });
            }

            var newUser = new User
            {
                Email = dto.Email,
                Password = BCryptPasswordHasher.HashPassword(dto.Password),
                BirthDate = dto.BirthDate,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Username = dto.Username,
                Role = int.Parse(UserRoleConstants.Customer),
                IsBanned = 0,
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var newCustomer = new Customer
            {
                UserId = newUser.Id,
                Ranking = "Đồng"
            };

            await _context.Customers.AddAsync(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
                Message = "Đăng ký thành công."
            });

        }

        // Otp sending endpoints.
        [HttpPost]
        [Route("password")]
        public async Task<IActionResult> ForgotPassword(ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return BadRequest(new BaseApiResponse
                    {
                        IsSuccessful = false,
                        Message = "User not found."
                    });
                }

                string randomVerificationCode = VerificationCodeGenerator.GenerateCode();

                string mailContent =
                    $"<p> Chúng tôi vừa nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>" +
                    $" {user.Username} </strong>. </p>" +
                    $"<p> Sử dụng mã sau để thiết lập lại mật khẩu mới: " +
                    $"<strong>{randomVerificationCode}</strong>. </p>" +
                    $"<p> <i>Lưu ý: Mã OTP sẽ hết hạn sau {OTP_EXPIRATION_MINUTES} phút. </i> </p>";

                await _emailService.SendEmailAsync(dto.Email, "Đặt lại mật khẩu", mailContent);

                // Save Otp code to memory cache.
                _cacheUtils.SaveResetCode(dto.Email, randomVerificationCode, OTP_EXPIRATION_MINUTES);

                return Ok(new BaseApiResponse
                {
                    IsSuccessful = true,
                    Message = "Gửi email thành công."
                });
            }

            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // Otp verification endpoint.
        [HttpPost]
        [Route("verify-reset")]
        public IActionResult VerifyResetPasswordOTP(VerifyPasswordOtpDto dto)
        {
            var isValid = _cacheUtils.VerifyResetCode(dto.Email, dto.Otp);

            if (!isValid)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Mã OTP không hợp lệ hoặc đã hết hạn."
                });
            }

            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
                Message = "Xác thực thành công."
            });
        }

        // Reset password endpoint.
        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(UpdatePasswordDto dto)
        {
            try
            {
                var isValid = _cacheUtils.VerifyResetCode(dto.Email, dto.Otp);

                if (!isValid)
                {
                    return BadRequest(new BaseApiResponse
                    {
                        IsSuccessful = false,
                        Message = "Yêu cầu không hợp lệ hoặc đã hết hạn."
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return BadRequest(new BaseApiResponse
                    {
                        IsSuccessful = false,
                        Message = "Không tìm thấy người dùng."
                    });
                }

                // Update user's password.
                user.Password = BCryptPasswordHasher.HashPassword(dto.Password);
                await _context.SaveChangesAsync();

                // Remove the OTP code from cache.
                _cacheUtils.RemoveResetCode(dto.Email);

                return Ok(new BaseApiResponse
                {
                    IsSuccessful = true,
                    Message = "Cập nhật mật khẩu thành công."
                });
            }

            catch (Exception e)
            {
                return StatusCode(500, new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = e.Message,
                });
            }
        }

        [HttpGet]
        [Route("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfileInfo()
        {
            var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdRaw))
            {
                return NotFound();
            }

            _ = int.TryParse(userIdRaw, out var userId);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = userId,
                FullName = user.FullName,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Email = user.Email,
                IsBanned = user.IsBanned,
                Password = user.Password,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Username = user.Username,
            };

            return Ok(new ApiResult<UserDto>
            {
                IsSuccessful = true,
                Data = userDto
            });
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Auth")
            };

            if (!string.IsNullOrEmpty(returnUrl))
            {
                properties.Items["returnUrl"] = returnUrl;
            }

            return Challenge(properties, "Google");
        }

        [HttpGet("GoogleCallback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Cookies");

            if (!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var givenName = authenticateResult.Principal.FindFirst(ClaimTypes.GivenName)?.Value;

            if (email == null)
            {
                return BadRequest("No email received from Google.");
            }

            // Automatically bind the user account to the Google account,
            // or create new user.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Password = null,
                    FullName = givenName!,
                    Username = name!,
                    PhoneNumber = null,
                    BirthDate = null,
                    LoginProvider = LoginProviderConstant.GoogleLogin,
                    Role = int.Parse(UserRoleConstants.Customer),
                    IsBanned = 0,
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var customer = new Customer
                {
                    UserId = user.Id,
                    Ranking = "Đồng",
                };
                await _context.Customers.AddAsync(customer);
            }

            else
            {
                if (user.IsBanned == 1)
                {
                    var errorMessage = "Tài khoản của bạn đã bị chặn!";
                    var url = $"https://localhost:7127/Users/GoogleLoginFailed?message={Uri.EscapeDataString(errorMessage)}";
                    return Redirect(url);
                }

                user.LoginProvider = LoginProviderConstant.GoogleLogin;
            }

            await _context.SaveChangesAsync();

            var jwtToken = _jwtTokenProvider.CreateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(5)
            };

            Response.Cookies.Append(JWT_COOKIE_NAME, jwtToken, cookieOptions);

            string? returnUrl = null;

            if (authenticateResult.Properties != null &&
                authenticateResult.Properties.Items.TryGetValue("returnUrl", out var rawReturnUrl))
            {
                returnUrl = rawReturnUrl;
            }

            var redirectUrl = $"https://localhost:7127/Users/GoogleLoginSuccess?token={Uri.EscapeDataString(jwtToken)}";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                redirectUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
            }

            return Redirect(redirectUrl);

        }
    }
}
