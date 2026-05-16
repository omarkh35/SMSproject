using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Auth
{
    public class LoginRequestDTO
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
