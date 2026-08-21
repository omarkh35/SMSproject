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







        public async Task SendForgotPasswordOtpAsync(string toEmail, string recipientName, string otpCode)
        {
            try
            {
                var schoolName = _configuration["EmailSettings:DisplayName"] ?? _configuration["SchoolInfo:NameAr"] ?? "نظام إدارة المدرسة";
                var senderEmail = _configuration["EmailSettings:Email"] ?? "no-reply@school.edu";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(schoolName, senderEmail));
                message.To.Add(new MailboxAddress(recipientName, toEmail));
                message.Subject = $"رمز إعادة تعيين كلمة المرور - [{otpCode}]";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div dir='rtl' style='font-family: Arial, Tahoma, sans-serif; max-width: 550px; margin: auto; padding: 25px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #ffffff;'>
                        <h2 style='color: #0284c7; text-align: center; margin-bottom: 20px;'>{schoolName}</h2>
                        <p style='font-size: 15px; color: #334155;'>مرحباً <strong>{recipientName}</strong>،</p>
                        <p style='font-size: 14px; color: #475569; line-height: 1.6;'>
                            تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك في النظام. استخدم رمز التحقق المؤقت التالي لإتمام العملية:
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <span style='background: #f0f9ff; border: 2px dashed #0284c7; border-radius: 8px; padding: 12px 28px; font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #0369a1; font-family: monospace; display: inline-block;'>
                                {otpCode}
                            </span>
                            <p style='color: #dc2626; font-size: 12px; margin-top: 10px;'>⏳ هذا الرمز صالح لمدة 10 دقائق فقط.</p>
                        </div>
                        <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 20px 0;' />
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>إذا لم تكن أنت من طلب هذا الرمز، يُرجى تجاهل هذه الرسالة وأمان حسابك في أمان تام.</p>
                    </div>",
                    TextBody = $@"مرحباً {recipientName}،

لقد طلبت إعادة تعيين كلمة المرور الخاصة بحسابك في {schoolName}.
رمز التحقق الخاص بك هو: {otpCode}

ملاحظة: هذا الرمز صالح لمدة 10 دقائق فقط.
إذا لم تكن أنت من طلب هذا الرمز، يرجى تجاهل هذا البريد الإلكتروني.
--
{schoolName}"
                };

                message.Body = bodyBuilder.ToMessageBody();

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
                throw new InvalidOperationException($"فشل إرسال بريد استعادة كلمة المرور: {ex.Message}", ex);
            }
        }





    }
}
