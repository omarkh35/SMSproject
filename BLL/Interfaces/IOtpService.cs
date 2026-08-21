using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(string key);
        bool ValidateOtp(string key, string otp);
        void RemoveOtp(string key);
        int GetFailedAttempts(string key);
        void IncrementFailedAttempts(string key);
        void ResetFailedAttempts(string key);
        bool IsBlocked(string key);

        // دوال مخصصة لتفعيل الحساب وحفظ كلمة المرور المعلقة في الكاش بأمان
        string StorePendingActivation(string accountNumber, string hashedPassword, int userId);
        bool ValidateActivationOtp(string accountNumber, string otp, out string? hashedPassword, out int userId);
        void ClearPendingActivation(string accountNumber);

        string StoreResetPasswordOtp(string email);
        bool ValidateResetPasswordOtp(string email, string otp, out string? errorMessage);
        bool IsResetOtpVerified(string email);
        void ClearResetPassword(string email);
    }
}
