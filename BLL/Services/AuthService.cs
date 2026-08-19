using BLL.EntitiesDTOS.Auth;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<UserRefreshToken> _refreshTokenRepo;
        private readonly IJwtService _jwtService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;


        public AuthService(
            IBaseRepositories<User> userRepo,
            IBaseRepositories<UserRefreshToken> refreshTokenRepo,
            IJwtService jwtService, IOtpService otpService,
        IEmailService emailService)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            
            _jwtService = jwtService;
            _emailService = emailService;
            _otpService = otpService;


        }


        public async Task<TokenResponseDto?> LoginAsync(LoginRequestDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.AccountNumber) || string.IsNullOrWhiteSpace(loginDto.AccountNumber) || string.IsNullOrWhiteSpace(loginDto.Password))
                return null;

            string cleanAccountNumber = loginDto.AccountNumber.Trim();

            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
                u => u.AccountNumber == cleanAccountNumber,
                u => u.Person,
                u => u.UserRole
            );


            var user = users.FirstOrDefault();


            if (user == null || user.Person == null || !user.Person.IsActive)
                return null;

            //var passCheck = _passwordHasher.VerifyHashedPassword(user, user.PassHash, loginDto.Password);

            //if (passCheck == PasswordVerificationResult.Failed)
            //    return null;
            //var passCheck = (loginDto.Password == user.HashPassword);
            //if (!passCheck)
            //    return null;

            if (string.IsNullOrEmpty(user.HashPassword))
            {
                if (loginDto.Password.Length < 6)
                {
                    throw new InvalidOperationException("يجب ألا تقل كلمة المرور عن 6 خانات.");
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    throw new InvalidOperationException("لا يوجد بريد إلكتروني مسجل لهذا الحساب للتواصل. يرجى مراجعة إدارة المدرسة.");
                }

                if (_otpService.IsBlocked(cleanAccountNumber))
                {
                    throw new InvalidOperationException("لقد تم حظر طلبات التحقق لهذا الحساب مؤقتاً بسبب تكرار المحاولات الخاطئة. يرجى المحاولة بعد 5 دقائق.");
                }

                // تشفير كلمة المرور وتخزين الطلب المعلق في الذاكرة (MemoryCache)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginDto.Password.Trim());
                string otp = _otpService.StorePendingActivation(cleanAccountNumber, hashedPassword, user.UserId);

                // إرسال رمز الـ OTP إلى البريد الإلكتروني للمستخدم
                await _emailService.SendOtpAsync(user.Email, otp);

                string maskedEmail = MaskEmail(user.Email);

                return new TokenResponseDto
                {
                    RequiresOtp = true,
                    MaskedEmail = maskedEmail,
                    Message = $"هذا الحساب جديد، تم إرسال رمز التحقق (OTP) إلى بريدك الإلكتروني المسجل ({maskedEmail}) لتأكيد تعيين كلمة المرور وتفعيل الحساب.",
                    User = null,
                    AccessToken = null,
                    RefreshToken = null
                };
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashPassword);
                if (!isPasswordValid)
                    return null; 
            


            var token = _jwtService.GenerateToken(user);

          
            var refreshToken = new UserRefreshToken
            {
                TokenValue = token.RefreshToken,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                UserId = user.UserId
            };

            await _refreshTokenRepo.AddAsync(refreshToken);


            await _refreshTokenRepo.SaveChangesAsync();

            return new TokenResponseDto
            {
                RequiresOtp = false,
                Message = "تم تسجيل الدخول بنجاح.",
                User = new EntitiesDTOS.User.UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.UserRole?.RoleName ?? "No Role",
                    UserID = user.UserId
                },
                AccessToken = token.AccessToken,
                RefreshToken = refreshToken.TokenValue
            };
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshRequestDto refreshDto)
        {
            if (refreshDto == null || string.IsNullOrEmpty(refreshDto.RefreshToken))
                return null;

            var refreshTokens = await _refreshTokenRepo.GetAllWithIncludeAndFilterAsync(
                t => t.TokenValue == refreshDto.RefreshToken,
                t => t.User
            );

            var currentRefreshToken = refreshTokens.FirstOrDefault();

            if (currentRefreshToken == null || !currentRefreshToken.IsActive)
                return null; 

            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
                u => u.UserId == currentRefreshToken.UserId,
                u => u.Person,
                u => u.UserRole
            );

            var user = users.FirstOrDefault();
            if (user == null || user.Person == null || !user.Person.IsActive)
                return null;

          
            var tokenResponse = _jwtService.GenerateToken(user);

            
            currentRefreshToken.RevokedOn = DateTime.UtcNow;
            //currentRefreshToken.ReplacedByToken = tokenResponse.RefreshToken;
            _refreshTokenRepo.UpdateAsync(currentRefreshToken);

            
            var newRefreshToken = new UserRefreshToken
            {
                TokenValue = tokenResponse.RefreshToken, 
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                UserId = user.UserId
            };
            await _refreshTokenRepo.AddAsync(newRefreshToken);

            await _refreshTokenRepo.SaveChangesAsync();

            return new TokenResponseDto
            {
                RequiresOtp = false,
                Message = "تم تجديد الجلسة بنجاح.",
                User = new EntitiesDTOS.User.UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.UserRole?.RoleName ?? "No Role",
                    UserID = user.UserId
                },
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken
            };

           
        }

        public async Task<bool> LogoutAsync(LogoutRequestDto logoutDto)
        {
            if (logoutDto == null || string.IsNullOrEmpty(logoutDto.RefreshToken))
                return false;

            var refreshTokens = await _refreshTokenRepo.GetAllWithIncludeAndFilterAsync(
                t => t.TokenValue == logoutDto.RefreshToken
            );

            var currentRefreshToken = refreshTokens.FirstOrDefault();

            if (currentRefreshToken == null || !currentRefreshToken.IsActive)
                return false;

            currentRefreshToken.RevokedOn = DateTime.UtcNow;

            _refreshTokenRepo.UpdateAsync(currentRefreshToken);
            await _refreshTokenRepo.SaveChangesAsync();

            return true;
        }




        //public async Task<AccountActivationResponseDto> RequestAccountActivationOtpAsync(AccountActivationRequestDto dto)
        //{
        //    if (dto == null || string.IsNullOrWhiteSpace(dto.AccountNumber))
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "رقم الحساب مطلوب."
        //        };
        //    }

        //    if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "يجب ألا تقل كلمة المرور الجديدة عن 6 خانات."
        //        };
        //    }

        //    string cleanAccountNumber = dto.AccountNumber.Trim();

        //    // 1. فحص وجود الحساب في قاعدة البيانات
        //    var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
        //        u => u.AccountNumber == cleanAccountNumber,
        //        u => u.Person,
        //        u => u.UserRole
        //    );

        //    var user = users.FirstOrDefault();
        //    if (user == null)
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "رقم الحساب المدخل غير موجود في النظام."
        //        };
        //    }

        //    if (user.Person == null || !user.Person.IsActive)
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "هذا الحساب غير نشط حالياً، يرجى مراجعة إدارة المدرسة."
        //        };
        //    }

        //    // 2. فحص هل الحساب مفعّل ولديه كلمة مرور مسبقاً
        //    if (!string.IsNullOrEmpty(user.HashPassword))
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "تم تفعيل هذا الحساب مسبقاً ولديه كلمة مرور بالفعل. يرجى تسجيل الدخول مباشرة أو طلب إعادة تعيين كلمة المرور."
        //        };
        //    }

        //    // 3. فحص وجود بريد إلكتروني مسجل في قاعدة البيانات
        //    if (string.IsNullOrWhiteSpace(user.Email))
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "لا يوجد بريد إلكتروني مسجل لهذا الحساب للتواصل. يرجى مراجعة إدارة المدرسة لتحديث بياناتك."
        //        };
        //    }

        //    // 4. فحص هل الحساب محظور مؤقتاً بسبب تجاوز المحاولات الخاطئة
        //    if (_otpService.IsBlocked(cleanAccountNumber))
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = "لقد تم حظر طلبات التحقق لهذا الحساب مؤقتاً بسبب تكرار المحاولات الخاطئة. يرجى المحاولة بعد 5 دقائق."
        //        };
        //    }

        //    try
        //    {
        //        // 5. تشفير كلمة المرور وتخزين الطلب المعلق في الذاكرة (MemoryCache)
        //        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword.Trim());
        //        string otp = _otpService.StorePendingActivation(cleanAccountNumber, hashedPassword, user.UserId);

        //        // 6. إرسال رمز الـ OTP إلى البريد الإلكتروني للمستخدم
        //        await _emailService.SendOtpAsync(user.Email, otp);

        //        string maskedEmail = MaskEmail(user.Email);

        //        return new AccountActivationResponseDto
        //        {
        //            Success = true,
        //            Message = $"تم إرسال رمز التحقق بنجاح إلى بريدك الإلكتروني المسجل ({maskedEmail}).",
        //            MaskedEmail = maskedEmail,
        //            ExpiryMinutes = 5
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new AccountActivationResponseDto
        //        {
        //            Success = false,
        //            Message = $"فشل إرسال رمز التحقق: {ex.Message}"
        //        };
        //    }
        //}


        public async Task<TokenResponseDto?> VerifyLoginOtpAsync(AccountActivationVerifyDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.AccountNumber) || string.IsNullOrWhiteSpace(dto.Otp))
                return null;

            string cleanAccountNumber = dto.AccountNumber.Trim();
            string cleanOtp = dto.Otp.Trim();

            if (_otpService.IsBlocked(cleanAccountNumber))
            {
                throw new InvalidOperationException("لقد تم حظر هذا الحساب مؤقتاً لتجاوز عدد المحاولات الخاطئة. يرجى المحاولة بعد 5 دقائق.");

            }

            // 1. التحقق من صحة الرمز واسترجاع كلمة المرور المشفرة والمعرف المخزن في الكاش
            bool isValid = _otpService.ValidateActivationOtp(cleanAccountNumber, cleanOtp, out string? pendingHashPassword, out int userId);

            if (!isValid || string.IsNullOrEmpty(pendingHashPassword) || userId == 0)
            {
                if (_otpService.IsBlocked(cleanAccountNumber))
                {
                    throw new InvalidOperationException("لقد تم حظر طلبات التحقق لهذا الحساب لتجاوز عدد المحاولات المسموحة (3 محاولات). يرجى المحاولة بعد 5 دقائق.");

                }

                int remainingAttempts = 3 - _otpService.GetFailedAttempts(cleanAccountNumber);
                string errorMsg = remainingAttempts > 0
                    ? $"رمز التحقق غير صحيح أو منتهي الصلاحية. تبقت لك {remainingAttempts} محاولات."
                    : "انتهت صلاحية رمز التحقق أو لم يتم العثور على طلب معلق. يرجى محاولة تسجيل الدخول مجدداً.";

                throw new ArgumentException(errorMsg);
            }

            // 2. جلب المستخدم من قاعدة البيانات للتأكد من الحالة
            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
                u => u.UserId == userId || u.AccountNumber == cleanAccountNumber,
                u => u.Person,
                u => u.UserRole
            );
            var user = users.FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            // 3. حفظ كلمة المرور المشفرة بشكل دائم في قاعدة البيانات
            user.HashPassword = pendingHashPassword;
            _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            // 4. توليد JWT Token تلقائي للمستخدم ليتمكن من الدخول مباشرة
            var token = _jwtService.GenerateToken(user);
            var refreshToken = new UserRefreshToken
            {
                TokenValue = token.RefreshToken,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                UserId = user.UserId
            };
            await _refreshTokenRepo.AddAsync(refreshToken);
            await _refreshTokenRepo.SaveChangesAsync();

            return new TokenResponseDto
            {
                RequiresOtp = false,
                Message = "تم تفعيل الحساب وتعيين كلمة المرور بنجاح.",
                User = new EntitiesDTOS.User.UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.UserRole?.RoleName ?? "No Role",
                    UserID = user.UserId
                },
                AccessToken = token.AccessToken,
                RefreshToken = refreshToken.TokenValue
            };

        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            string name = parts[0];
            string domain = parts[1];

            if (name.Length <= 2)
                return $"{name}***@{domain}";

            return $"{name.Substring(0, 2)}***{name.Substring(name.Length - 1)}@{domain}";
        }

    }
}
