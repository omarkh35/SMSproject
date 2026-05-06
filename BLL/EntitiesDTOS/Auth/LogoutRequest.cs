using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Auth
{
    public class LogoutRequest
    {
        public string PhoneNumber { get; set; }
        public string RefreshToken { get; set; }
    }
}
