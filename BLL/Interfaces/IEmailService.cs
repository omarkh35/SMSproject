using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string plainTextBody);
        Task SendUserNumberAsync(string email, string number);
        Task SendOtpAsync(string email, string otp);
        Task SendForgotPasswordOtpAsync(string toEmail, string recipientName, string otpCode);

    }
}
