using BLL.EntitiesDTOS.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminAuthService
    {

        Task<AdminTokenResponseDto?> AdminLoginAsync(AdminLoginRequestDto loginDto);


    }
}
