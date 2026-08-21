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

        Task<TokenResponseDto?> VerifyLoginOtpAsync(AccountActivationVerifyDto verifyDto);


        //    Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email);
        //    Task<(bool Success, string Message, string? ResetToken)> VerifyOtpAsync(VerifyOtpDto dto);
        //    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);

        Task<(bool Success, string Message, string? MaskedEmail)> SendForgotPasswordOtpAsync(string emailOrAccountNumber);
        Task<(bool Success, string Message)> VerifyResetOtpAsync(VerifyResetOtpDto dto);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
