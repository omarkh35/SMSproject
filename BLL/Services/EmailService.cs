using BLL.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string plainTextBody)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:DisplayName"],
                    _configuration["EmailSettings:Email"]
                ));

                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = plainTextBody };

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل إرسال الإيميل: {ex.Message}", ex);
            }
        }

        public async Task SendUserNumberAsync(string email, string number)
        {
            var plainText = $@"
مرحباً ,

تم تسجيلك  بنجاح في نظام  المدرسة.

رقم التعريفي الخاص هو: {number}

يرجى الاحتفاظ بهذا الرقم.

            ";

            await SendEmailAsync(email, "رقمك الخاص ", plainText);
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var plainText = $@"
مرحباً ,

لقد طلبت تسجيل الدخول إلى نظام إدارة المدرسة.

رمز التحقق الخاص بك هو: {otp}

 هذا الرمز صالح لمدة 5 دقائق فقط.

إذا لم تطلب هذا الرمز، يرجى تجاهل هذا البريد.

--
            ";

            await SendEmailAsync(email, "🔐 رمز التحقق الخاص بتسجيل الدخول", plainText);
        }
    }
}
