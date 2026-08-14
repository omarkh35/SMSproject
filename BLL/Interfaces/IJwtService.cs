using BLL.EntitiesDTOS;
using BLL.EntitiesDTOS.Auth;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BLL.Interfaces
{
    public interface IJwtService
    {
        TokenResponseDto GenerateToken(User user);
        TokenResponseDto GenerateToken(string username, string roleName);

        // Task<TokenResponseModel> RefreshTokenAsync(string refreshToken)
    }
}
