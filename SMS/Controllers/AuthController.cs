using BLL.EntitiesDTOS.Auth;
using BLL.EntitiesDTOS.General;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IResetPassService _resetPassService;

        public AuthController(IAuthService authService, IResetPassService esetPassService)
        {
            _authService = authService;
            _resetPassService = esetPassService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(request);

                if (result == null)
                    return Unauthorized(new { message = "بيانات الدخول غير صحيحة أو أن الحساب غير نشط." });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"حدث خطأ أثناء معالجة الطلب: {ex.Message}" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] AccountActivationVerifyDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.VerifyLoginOtpAsync(request);

                if (result == null)
                    return NotFound(new { message = "تعذر العثور على الحساب أو إتمام التفعيل." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"حدث خطأ أثناء التحقق: {ex.Message}" });
            }
        }



        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            return Ok(result);
        }

        //منحطها بعد ما نحط ال Baerer
        //[Authorize] 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _authService.LogoutAsync(request);

            if (!result)
            {
                return BadRequest(new { message = "Invalid or already revoked refresh token." });
            }

            return Ok(new { message = "Logged out successfuly. Token has been revoked." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء تسجيل الخروج.",
                    details = ex.Message
                });
            }
        }



        // =========================================================================
        // 1. طلب استعادة كلمة المرور وإرسال الـ OTP (بدون توكن - AllowAnonymous)
        // =========================================================================
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "البريد الإلكتروني المدخل غير صالح.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var (success, message, maskedEmail) = await _authService.SendForgotPasswordOtpAsync(request.Email);
                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new
                {
                    success = true,
                    message,
                    maskedEmail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"حدث خطأ أثناء إرسال رمز التحقق: {ex.Message}"
                });
            }
        }

        // =========================================================================
        // 2. التحقق من صحة رمز الـ OTP لإعادة تعيين كلمة المرور
        // =========================================================================
        [HttpPost("verify-reset-otp")]
        [HttpPost("verify-otp-fro-reset-pass")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "البيانات المدخلة غير مكتملة أو غير صالحة.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var (success, message) = await _authService.VerifyResetOtpAsync(dto);
                if (!success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message,
                    email = dto.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"حدث خطأ أثناء التحقق من الرمز: {ex.Message}"
                });
            }
        }

        // =========================================================================
        // 3. تعيين وحفظ كلمة المرور الجديدة في قاعدة البيانات
        // =========================================================================
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "البيانات المدخلة غير مكتملة أو غير متطابقة.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var (success, message) = await _authService.ResetPasswordAsync(request);
                if (!success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"حدث خطأ أثناء إعادة تعيين كلمة المرور: {ex.Message}"
                });
            }
        }
    }
}


   
