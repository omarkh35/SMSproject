using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.EntitiesDTOS.Auth;

namespace BLL.Interfaces
{
    public interface IResetPassService
    {
        Task SendOtpEmailAsync(string toEmail, string recipientName, string otpCode);
        Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email);
        Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
