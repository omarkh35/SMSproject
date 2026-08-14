using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Admin
{
    public class AdminLoginRequestDto
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;
    }

    public class AdminTokenResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "SuperAdmin";
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

        
}
