using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public OtpController(IOtpService otpService, IEmailService emailService)
        {
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] JsonElement request)
        {
            try
            {
              
                if (!request.TryGetProperty("email", out JsonElement emailElement))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                string email = emailElement.GetString();

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                if (_otpService.IsBlocked(email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "لقد تجاوزت عدد المحاولات المسموحة. يرجى المحاولة بعد 5 دقائق."
                    });
                }

                var otp = _otpService.GenerateOtp(email);

                await _emailService.SendOtpAsync(email, otp);

                return Ok(new
                {
                    Success = true,
                    Message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني",
                    Email = email,
                    ExpiryMinutes = 5
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"حدث خطأ: {ex.Message}"
                });
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] JsonElement request)
        {
            try
            {
                if (!request.TryGetProperty("email", out JsonElement emailElement))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                if (!request.TryGetProperty("otp", out JsonElement otpElement))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "رمز التحقق مطلوب"
                    });
                }

                string email = emailElement.GetString();
                string otp = otpElement.GetString();

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                if (string.IsNullOrEmpty(otp))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "رمز التحقق مطلوب"
                    });
                }

                var isValid = _otpService.ValidateOtp(email, otp);

                if (isValid)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "تم التحقق بنجاح",
                        Verified = true,
                        Email = email
                    });
                }
                else
                {
                    if (_otpService.IsBlocked(email))
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "لقد تجاوزت عدد المحاولات المسموحة. يرجى المحاولة بعد 5 دقائق."
                        });
                    }

                    var remainingAttempts = 3 - _otpService.GetFailedAttempts(email);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"رمز التحقق غير صحيح. تبقت لك {remainingAttempts} محاولة."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"حدث خطأ: {ex.Message}"
                });
            }
        }

        [HttpPost("send-personal-number")]
        public async Task<IActionResult> SendPersonalNumber([FromBody] JsonElement request)
        {
            try
            {
                if (!request.TryGetProperty("email", out JsonElement emailElement))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                if (!request.TryGetProperty("personalNumber", out JsonElement numberElement))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "الرقم الشخصي مطلوب"
                    });
                }

                string email = emailElement.GetString();
                string personalNumber = numberElement.GetString();

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مطلوب"
                    });
                }

                if (string.IsNullOrEmpty(personalNumber))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "الرقم الشخصي مطلوب"
                    });
                }

                await _emailService.SendUserNumberAsync(email, personalNumber);

                return Ok(new
                {
                    Success = true,
                    Message = "تم إرسال الرقم الشخصي إلى بريدك الإلكتروني",
                    Email = email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"حدث خطأ: {ex.Message}"
                });
            }
        }
    }
}