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
    }

}
