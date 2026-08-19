using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.EntitiesDTOS.User;


namespace BLL.EntitiesDTOS.Auth
{
    public class TokenResponseDto
    {
        public bool RequiresOtp { get; set; } = false;
        public string? MaskedEmail { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
