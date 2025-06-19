using BusinessObjects.ApiResponses;
using BusinessObjects.Constants;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SunStoreAPI.Dtos;
using SunStoreAPI.Services;
using SunStoreAPI.Utils;

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
                return BadRequest(loginResponseDto);
            }

            // 2. Check if user is banned.
            if (user.IsBanned == 1)
            {
                loginResponseDto.ErrorMessage = "Tài khoản của bạn đã bị chặn!";
                return Unauthorized(loginResponseDto);
            }

            // 3. Check password.
            if (dto.Password != user.Password)
            {
                loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return Unauthorized(loginResponseDto);
            }

            //if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            //{
            //    loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            //    return Unauthorized(loginResponseDto);
            //}

            // 4. Create JWT.
            string jwtToken = _jwtTokenProvider.CreateToken(user);

            // 5. Set JWT in cookie.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
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
                return BadRequest(new ApiResult
                {
                    IsSuccessful = false,
                    Message = "Email này đã tồn tại."
                });
            }

            var phoneExisted = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber) != null;

            if (phoneExisted)
            {
                return BadRequest(new ApiResult
                {
                    IsSuccessful = false,
                    Message = "Số điện thoại đã tồn tại."
                });
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new ApiResult
                {
                    IsSuccessful = false,
                    Message = "Mật khẩu không khớp!"
                });
            }

            var newUser = new User
            {
                Email = dto.Email,
                Password = dto.Password,
                BirthDate = dto.BirthDate,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Username = "", // Temp value to bypass null check.
                Role = int.Parse(UserRoleConstants.Customer),
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok(new ApiResult
            {
                IsSuccessful = true,
                Message = "Đăng ký thành công."
            });

        }

        [HttpPost]
        [Route("password")]
        public async Task<IActionResult> ForgotPassword(ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                string randomVerificationCode = VerificationCodeGenerator.GenerateCode();

                string mailContent = $"<p> Sử dụng mã sau để thiết lập lại mật khẩu mới:" +
                    $"<strong>{randomVerificationCode}</strong>. </p>" +
                    $"<p> <i>Lưu ý: Mã OTP sẽ hết hạn sau {OTP_EXPIRATION_MINUTES} phút. </i>";

                await _emailService.SendEmailAsync(dto.Email, "Đặt lại mật khẩu", mailContent);

                _cacheUtils.SaveResetCode(dto.Email, randomVerificationCode, OTP_EXPIRATION_MINUTES);

                return Ok("Gửi email thành công.");
            }

            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("verify-reset")]
        public IActionResult VerifyResetPasswordOTP(VerifyPasswordOtpDto dto)
        {
            var isValid = _cacheUtils.VerifyResetCode(dto.Email, dto.Otp);

            if (!isValid)
            {
                return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            return Ok("Xác thực thành công.");
        }
    }
}
