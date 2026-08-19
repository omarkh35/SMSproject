using BLL.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
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
                    _configuration["EmailSettings:DisplayName"] ?? "نظام إدارة المدرسة",
                    _configuration["EmailSettings:Email"]
                ));

                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = plainTextBody };

                using var client = new SmtpClient();

                string smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                int smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out int port) ? port : 587;

                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل إرسال البريد الإلكتروني: {ex.Message}", ex);
            }
        }

        public async Task SendUserNumberAsync(string email, string number)
        {
            var plainText = $@"
مرحباً،
تم تسجيلك بنجاح في نظام إدارة المدرسة.
رقمك التعريفي الخاص (Account Number) هو: {number}
يرجى الاحتفاظ بهذا الرقم لتسجيل الدخول وتفعيل حسابك.
--
إدارة المدرسة
            ";

            await SendEmailAsync(email, "رقمك التعريفي الخاص بالنظام", plainText);
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var plainText = $@"
مرحباً،
لقد طلبت تفعيل حسابك وتعيين كلمة المرور في نظام إدارة المدرسة.
رمز التحقق الخاص بك هو: {otp}

ملاحظة: هذا الرمز صالح لمدة 5 دقائق فقط.
إذا لم تطلب هذا الرمز، يرجى تجاهل هذا البريد الإلكتروني.
--
إدارة المدرسة
            ";

            await SendEmailAsync(email, "🔐 رمز التحقق الخاص بتفعيل الحساب", plainText);
        }
    }
}
