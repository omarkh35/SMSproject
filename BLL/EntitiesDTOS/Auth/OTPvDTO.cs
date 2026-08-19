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

    
}
