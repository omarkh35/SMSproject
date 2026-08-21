using BLL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();

        private const int OtpExpiryMinutes = 5;
        private const int MaxFailedAttempts = 3;
        private const int BlockDurationMinutes = 5;

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateOtp(string key)
        {
            RemoveOtp(key);

            var otp = _random.Next(100000, 999999).ToString();
            var otpKey = $"OTP_{key}";
            _cache.Set(otpKey, otp, TimeSpan.FromMinutes(OtpExpiryMinutes));

            ResetFailedAttempts(key);
            return otp;
        }

        public bool ValidateOtp(string key, string otp)
        {
            if (IsBlocked(key))
                return false;

            var otpKey = $"OTP_{key}";
            if (_cache.TryGetValue(otpKey, out string? storedOtp))
            {
                if (storedOtp == otp)
                {
                    RemoveOtp(key);
                    ResetFailedAttempts(key);
                    return true;
                }
                else
                {
                    IncrementFailedAttempts(key);
                    return false;
                }
            }

            IncrementFailedAttempts(key);
            return false;
        }

        public void RemoveOtp(string key)
        {
            var otpKey = $"OTP_{key}";
            _cache.Remove(otpKey);
        }

        public int GetFailedAttempts(string key)
        {
            var attemptsKey = $"OTP_Attempts_{key}";
            return _cache.TryGetValue(attemptsKey, out int attempts) ? attempts : 0;
        }

        public void IncrementFailedAttempts(string key)
        {
            var attemptsKey = $"OTP_Attempts_{key}";
            var attempts = GetFailedAttempts(key) + 1;
            _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(BlockDurationMinutes));
        }

        public void ResetFailedAttempts(string key)
        {
            var attemptsKey = $"OTP_Attempts_{key}";
            _cache.Remove(attemptsKey);
        }

        public bool IsBlocked(string key)
        {
            var attempts = GetFailedAttempts(key);
            return attempts >= MaxFailedAttempts;
        }

        // =========================================================================
        // تفعيل الحساب: تخزين الرمز وكلمة المرور المشفرة معاً في الذاكرة (MemoryCache)
        // =========================================================================
        public string StorePendingActivation(string accountNumber, string hashedPassword, int userId)
        {
            ClearPendingActivation(accountNumber);

            var otp = _random.Next(100000, 999999).ToString();
            var expiry = TimeSpan.FromMinutes(OtpExpiryMinutes);

            _cache.Set($"ACTIVATION_OTP_{accountNumber}", otp, expiry);
            _cache.Set($"ACTIVATION_HASH_{accountNumber}", hashedPassword, expiry);
            _cache.Set($"ACTIVATION_USERID_{accountNumber}", userId, expiry);

            ResetFailedAttempts(accountNumber);
            return otp;
        }

        public bool ValidateActivationOtp(string accountNumber, string otp, out string? hashedPassword, out int userId)
        {
            hashedPassword = null;
            userId = 0;

            if (IsBlocked(accountNumber))
                return false;

            if (_cache.TryGetValue($"ACTIVATION_OTP_{accountNumber}", out string? storedOtp))
            {
                if (storedOtp == otp)
                {
                    _cache.TryGetValue($"ACTIVATION_HASH_{accountNumber}", out hashedPassword);
                    _cache.TryGetValue($"ACTIVATION_USERID_{accountNumber}", out userId);

                    ClearPendingActivation(accountNumber);
                    ResetFailedAttempts(accountNumber);
                    return true;
                }
                else
                {
                    IncrementFailedAttempts(accountNumber);
                    return false;
                }
            }

            IncrementFailedAttempts(accountNumber);
            return false;
        }

        public void ClearPendingActivation(string accountNumber)
        {
            _cache.Remove($"ACTIVATION_OTP_{accountNumber}");
            _cache.Remove($"ACTIVATION_HASH_{accountNumber}");
            _cache.Remove($"ACTIVATION_USERID_{accountNumber}");
        }








        // =========================================================================
        // إعادة تعيين كلمة المرور: إدارة الـ OTP والتحقق وصلاحية الجلسة
        // =========================================================================
        public string StoreResetPasswordOtp(string email)
        {
            string cleanEmail = email.Trim().ToLower();
            ClearResetPassword(cleanEmail);

            var otp = _random.Next(100000, 999999).ToString();
            var expiry = TimeSpan.FromMinutes(10); // صالح لمدة 10 دقائق

            _cache.Set($"RESET_OTP_{cleanEmail}", otp, expiry);
            _cache.Set($"RESET_EXPIRY_{cleanEmail}", DateTime.UtcNow.Add(expiry), expiry);
            ResetFailedAttempts($"RESET_{cleanEmail}");

            return otp;
        }

        public bool ValidateResetPasswordOtp(string email, string otp, out string? errorMessage)
        {
            errorMessage = null;
            string cleanEmail = email.Trim().ToLower();
            string attemptKey = $"RESET_{cleanEmail}";

            if (IsBlocked(attemptKey))
            {
                errorMessage = "تم تجاوز الحد الأقصى للمحاولات الخاطئة. يُرجى طلب رمز جديد بعد 5 دقائق.";
                return false;
            }

            if (!_cache.TryGetValue($"RESET_OTP_{cleanEmail}", out string? storedOtp))
            {
                errorMessage = "انتهت صلاحية رمز التحقق أو لم يتم طلبه مسبقاً. يُرجى طلب رمز جديد.";
                return false;
            }

            if (storedOtp != otp.Trim())
            {
                IncrementFailedAttempts(attemptKey);
                int attempts = GetFailedAttempts(attemptKey);
                int remaining = Math.Max(0, MaxFailedAttempts - attempts);

                errorMessage = remaining > 0
                    ? $"رمز التحقق غير صحيح. (متبقي {remaining} محاولات)."
                    : "تم تجاوز الحد الأقصى للمحاولات الخاطئة. تم إلغاء الرمز، يُرجى طلب رمز جديد.";

                if (remaining == 0)
                {
                    ClearResetPassword(cleanEmail);
                }

                return false;
            }

            // تم التحقق بنجاح -> وضع علامة Verified لمدة 10 دقائق لإتاحة خطوة تغيير كلمة المرور
            _cache.Set($"RESET_VERIFIED_{cleanEmail}", true, TimeSpan.FromMinutes(10));
            ResetFailedAttempts(attemptKey);
            return true;
        }

        public bool IsResetOtpVerified(string email)
        {
            string cleanEmail = email.Trim().ToLower();
            return _cache.TryGetValue($"RESET_VERIFIED_{cleanEmail}", out bool isVerified) && isVerified;
        }

        public void ClearResetPassword(string email)
        {
            string cleanEmail = email.Trim().ToLower();
            _cache.Remove($"RESET_OTP_{cleanEmail}");
            _cache.Remove($"RESET_EXPIRY_{cleanEmail}");
            _cache.Remove($"RESET_VERIFIED_{cleanEmail}");
            _cache.Remove($"OTP_Attempts_RESET_{cleanEmail}");
        }








    }

}
