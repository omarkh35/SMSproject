using BLL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

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

        public string GenerateOtp(string email)
        {
            RemoveOtp(email);

            var otp = _random.Next(100000, 999999).ToString();
            var otpKey = $"OTP_{email}";
            _cache.Set(otpKey, otp, TimeSpan.FromMinutes(OtpExpiryMinutes));

            ResetFailedAttempts(email);

            return otp;
        }

        public bool ValidateOtp(string email, string otp)
        {
            if (IsBlocked(email))
                return false;

            var otpKey = $"OTP_{email}";
            if (_cache.TryGetValue(otpKey, out string storedOtp))
            {
                if (storedOtp == otp)
                {
                    RemoveOtp(email);
                    ResetFailedAttempts(email);
                    return true;
                }
                else
                {
                    IncrementFailedAttempts(email);
                    return false;
                }
            }

            IncrementFailedAttempts(email);
            return false;
        }

        public void RemoveOtp(string email)
        {
            var otpKey = $"OTP_{email}";
            _cache.Remove(otpKey);
        }

        public int GetFailedAttempts(string email)
        {
            var attemptsKey = $"OTP_Attempts_{email}";
            return _cache.TryGetValue(attemptsKey, out int attempts) ? attempts : 0;
        }

        public void IncrementFailedAttempts(string email)
        {
            var attemptsKey = $"OTP_Attempts_{email}";
            var attempts = GetFailedAttempts(email) + 1;
            _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(BlockDurationMinutes));
        }

        public void ResetFailedAttempts(string email)
        {
            var attemptsKey = $"OTP_Attempts_{email}";
            _cache.Remove(attemptsKey);
        }

        public bool IsBlocked(string email)
        {
            var attempts = GetFailedAttempts(email);
            return attempts >= MaxFailedAttempts;
        }
    }
}