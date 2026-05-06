using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Auth
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
        public string PhoneNumber { get; set; }
    }
}
