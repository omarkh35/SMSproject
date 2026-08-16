using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(string email);
        bool ValidateOtp(string email, string otp);
        void RemoveOtp(string email);
        int GetFailedAttempts(string email);
        void IncrementFailedAttempts(string email);
        void ResetFailedAttempts(string email);
        bool IsBlocked(string email);
    }
}
