using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Auth
{
   

    public class AccountActivationVerifyDto
    {
        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز التحقق (OTP) مطلوب")]
        public string Otp { get; set; } = string.Empty;
    }
    public class OtpCacheItem
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public int FailedAttempts { get; set; } = 0;
        public bool IsVerified { get; set; } = false;
        public string? ResetToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
