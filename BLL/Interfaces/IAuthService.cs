using BLL.EntitiesDTOS.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginRequestDTO loginDto);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshRequestDto refreshDto);

        Task<bool> LogoutAsync(LogoutRequestDto logoutDto);

        //Task<UserDto> RegisterAsync(CreateUserDto dto);
    }
}
