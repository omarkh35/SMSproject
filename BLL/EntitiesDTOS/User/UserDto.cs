using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.User
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Role { get; set; } = string.Empty;
        
        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; }  = string.Empty;

    }
}
