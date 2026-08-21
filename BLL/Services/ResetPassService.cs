using BLL.EntitiesDTOS;
using BLL.EntitiesDTOS.Auth;
using BLL.Interfaces;
using DAL.Context;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ResetPassService : IResetPassService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService; // خدمة إرسال الإيميل لديك

        // 2. تخزين الـ OTP في الذاكرة العشوائية (Thread-Safe Dictionary) بدون Cache وبدون داتابيز
        private static readonly ConcurrentDictionary<string, OtpItem> _otpStore = new();
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
        private const int MaxFailedAttempts = 5;


        private class OtpItem
        {
            public string OtpCode { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public int FailedAttempts { get; set; } = 0;
            public bool IsVerified { get; set; } = false;
        }

        public ResetPassService(AppDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string recipientName, string otpCode)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var senderEmail = _configuration["Smtp:SenderEmail"] ?? "no-reply@school.edu";
            var senderPassword = _configuration["Smtp:Password"] ?? "your-app-password";
            var schoolName = _configuration["SchoolInfo:NameAr"] ?? "نظام إدارة المدارس";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, schoolName),
                Subject = $"رمز إعادة تعيين كلمة المرور - [{otpCode}]",
                IsBodyHtml = true,
                Body = $@"
                <div dir='rtl' style='font-family: Arial, sans-serif; max-width: 550px; margin: auto; padding: 25px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #0284c7; text-align: center; margin-bottom: 20px;'>{schoolName}</h2>
                    <p style='font-size: 15px; color: #334155;'>مرحباً <strong>{recipientName}</strong>،</p>
                    <p style='font-size: 14px; color: #475569; line-height: 1.6;'>
                        تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك. استخدم رمز التحقق المؤقت التالي لإتمام العملية:
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='background: #f0f9ff; border: 2px dashed #0284c7; border-radius: 8px; padding: 12px 28px; font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #0369a1; font-family: monospace;'>
                            {otpCode}
                        </span>
                        <p style='color: #dc2626; font-size: 12px; margin-top: 10px;'>⏳ هذا الرمز صالح لمدة 10 دقائق فقط.</p>
                    </div>
                    <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #94a3b8; text-align: center;'>إذا لم تكن أنت من طلب هذا الرمز، يُرجى تجاهل هذه الرسالة.</p>
                </div>"
            };

            mailMessage.To.Add(new MailAddress(toEmail, recipientName));

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }




        // 1. استخدام DbContext الخاص بك بدلاً من UserManager


        
        public async Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email)
        {
            var cleanEmail = email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

            if (user == null)
            {
                return (false, "البريد الإلكتروني المدخل غير مسجل لدينا.");
            }

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var item = new OtpItem
            {
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.Add(OtpLifetime),
                FailedAttempts = 0,
                IsVerified = false
            };

            _otpStore.AddOrUpdate(cleanEmail, item, (key, old) => item);

            var recipientName = "مرحبا سيدي";
            await SendOtpEmailAsync(cleanEmail, recipientName, otpCode);

            return (true, "تم إرسال رمز التحقق إلى بريدك الإلكتروني بنجاح (صالح لمدة 10 دقائق).");
        }

       
        public Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var cleanEmail = dto.Email.Trim().ToLower();

            if (!_otpStore.TryGetValue(cleanEmail, out var item))
            {
                return Task.FromResult((false, "انتهت صلاحية رمز التحقق أو لم يتم طلبه مسبقاً."));
            }

            if (DateTime.UtcNow > item.ExpiresAt)
            {
                _otpStore.TryRemove(cleanEmail, out _);
                return Task.FromResult((false, "انتهت صلاحية رمز التحقق (10 دقائق). يُرجى طلب رمز جديد."));
            }

            if (item.FailedAttempts >= MaxFailedAttempts)
            {
                _otpStore.TryRemove(cleanEmail, out _);
                return Task.FromResult((false, "تم تجاوز الحد الأقصى للمحاولات الخاطئة. يُرجى طلب رمز جديد."));
            }


            if (item.OtpCode != dto.Otp.Trim())
            {
                item.FailedAttempts++;
                var remaining = MaxFailedAttempts - item.FailedAttempts;
                return Task.FromResult((false, $"رمز التحقق غير صحيح. (متبقي {remaining} محاولات)."));
            }

            item.IsVerified = true;
            return Task.FromResult((true, "تم التحقق من الرمز بنجاح! يمكنك الآن إدخال كلمة المرور الجديدة."));
        }

       
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var cleanEmail = dto.Email.Trim().ToLower();

            if (!_otpStore.TryGetValue(cleanEmail, out var item))
            {
                return (false, "انتهت صلاحية الجلسة، يُرجى طلب رمز تحقق جديد.");
            }

            if (!item.IsVerified || DateTime.UtcNow > item.ExpiresAt)
            {
                return (false, "يجب التحقق من رمز الـ OTP أولاً قبل تغيير كلمة المرور.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

            if (user == null)
            {
                return (false, "المستخدم غير موجود بالنظام.");
            }

            
            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            
            await _context.SaveChangesAsync();

            _otpStore.TryRemove(cleanEmail, out _);

            return (true, "تم تغيير كلمة المرور بنجاح وحفظها بدلاً من القديمة.");
        }
    }
}

   
      

