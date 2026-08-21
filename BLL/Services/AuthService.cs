using BLL.EntitiesDTOS.Auth;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IResetPassService _ResetPassService;


        public AuthService(
            IBaseRepositories<User> userRepo,
            IBaseRepositories<UserRefreshToken> refreshTokenRepo,
            IJwtService jwtService, IOtpService otpService,
        IEmailService emailService,IResetPassService resetPassService)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            
            _jwtService = jwtService;
            _emailService = emailService;
            _otpService = otpService;
            _ResetPassService = resetPassService;

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

        //    public async Task<(bool Success, string Message)> SendForgotPasswordOtpAsync(string email)
        //    {
        //        var cleanEmail = email.Trim().ToLower();

        //        // التحقق من وجود المستخدم في النظام عبر الإيميل فقط (بدون Account Number)
        //        var user = await _userManager.FindByEmailAsync(cleanEmail);
        //        if (user == null)
        //        {
        //            return (false, "البريد الإلكتروني المدخل غير مسجل لدينا.");
        //        }

        //        // توليد رمز رقمي عشوائي من 6 أرقام
        //        var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        //        // حفظ كائن الـ OTP في الكاش بمفتاح الإيميل مع صلاحية 10 دقائق
        //        var cacheKey = GetOtpCacheKey(cleanEmail);
        //        var cacheItem = new OtpCacheItem
        //        {
        //            Email = cleanEmail,
        //            OtpCode = otpCode,
        //            FailedAttempts = 0,
        //            IsVerified = false
        //        };

        //        var cacheEntryOptions = new MemoryCacheEntryOptions()
        //            .SetAbsoluteExpiration(OtpLifetime) // حذف تلقائي من الذاكرة بعد 10 دقائق
        //            .SetPriority(CacheItemPriority.High);

        //        _cache.Set(cacheKey, cacheItem, cacheEntryOptions);

        //        // إرسال الإيميل
        //        await _ResetPassService.SendOtpEmailAsync(cleanEmail, user.UserName ?? "User", otpCode);

        //        return (true, "تم إرسال رمز التحقق إلى بريدك الإلكتروني بنجاح (صالح لمدة 10 دقائق).");
        //    }

        //    // =========================================================================
        //    // 2. التحقق من صحة الـ OTP من داخل الكاش
        //    // =========================================================================
        //    public Task<(bool Success, string Message, string? ResetToken)> VerifyOtpAsync(VerifyOtpDto dto)
        //    {
        //        var cleanEmail = dto.Email.Trim().ToLower();
        //        var cacheKey = GetOtpCacheKey(cleanEmail);

        //        // 1. هل الكائن موجود في الكاش؟
        //        if (!_cache.TryGetValue(cacheKey, out OtpCacheItem? cacheItem) || cacheItem == null)
        //        {
        //            return Task.FromResult<(bool, string, string?)>((false, "انتهت صلاحية رمز التحقق أو لم يتم طلبه مسبقاً.", null));
        //        }

        //        // 2. فحص عدد المحاولات الخاطئة
        //        if (cacheItem.FailedAttempts >= MaxFailedAttempts)
        //        {
        //            _cache.Remove(cacheKey); // إتلاف الرمز فوراً للحماية
        //            return Task.FromResult<(bool, string, string?)>((false, "تم تجاوز الحد الأقصى للمحاولات الخاطئة. يُرجى طلب رمز جديد.", null));
        //        }

        //        if (cacheItem.OtpCode != dto.Otp.Trim())
        //        {
        //            cacheItem.FailedAttempts++;
        //            _cache.Set(cacheKey, cacheItem, OtpLifetime); // تحديث عدد المحاولات في الكاش
        //            var remaining = MaxFailedAttempts - cacheItem.FailedAttempts;
        //            return Task.FromResult<(bool, string, string?)>((false, $"رمز التحقق غير صحيح. (متبقي {remaining} محاولات)", null));
        //        }

        //        // 4. الرمز صحيح: توليد ResetToken مؤقت ووضع علامة Verified في الكاش
        //        var resetToken = Guid.NewGuid().ToString("N");
        //        cacheItem.IsVerified = true;
        //        cacheItem.ResetToken = resetToken;

        //        // إعادة حفظ الحالة الموثقة في الكاش لمدة 10 دقائق إضافية لإتاحة إدخال الباسورد
        //        _cache.Set(cacheKey, cacheItem, TimeSpan.FromMinutes(10));

        //        return Task.FromResult<(bool, string, string?)>((true, "تم التحقق من الرمز بنجاح.", resetToken));
        //    }

        //    // =========================================================================
        //    // 3. تعيين كلمة المرور الجديدة ومسح الـ OTP من الكاش
        //    // =========================================================================
        //    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        //    {
        //        var cleanEmail = dto.Email.Trim().ToLower();
        //        var cacheKey = GetOtpCacheKey(cleanEmail);

        //        // 1. فحص هل تم التحقق مسبقاً من الكاش
        //        if (!_cache.TryGetValue(cacheKey, out OtpCacheItem? cacheItem) || cacheItem == null)
        //        {
        //            return (false, "انتهت صلاحية الجلسة، يُرجى طلب رمز تحقق جديد.");
        //        }

        //        if (!cacheItem.IsVerified || cacheItem.OtpCode != dto.Otp.Trim())
        //        {
        //            return (false, "لم يتم التحقق من الرمز بنجاح أو أن الرمز غير متطابق.");
        //        }

        //        // 2. جلب المستخدم من الداتابيز فقط لتحديث كلمة المرور
        //        var user = await _userManager.FindByEmailAsync(cleanEmail);
        //        if (user == null)
        //        {
        //            return (false, "المستخدم غير موجود.");
        //        }

        //        // 3. تحديث كلمة المرور وحفظها في الداتابيز
        //        var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var result = await _userManager.ResetPasswordAsync(user, identityToken, dto.NewPassword);

        //        if (!result.Succeeded)
        //        {
        //            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        //            return (false, $"حدث خطأ أثناء تحديث كلمة المرور: {errors}");
        //        }

        //        // 4. حذف الـ OTP والـ Session من الـ MemoryCache تماماً لمنع أي استخدام مكرر
        //        _cache.Remove(cacheKey);

        //        return (true, "تم حفظ كلمة المرور الجديدة بنجاح بدلاً من القديمة.");
        //    }

        //    private static string GetOtpCacheKey(string email) => $"OTP_CACHE_{email}";




        // =========================================================================
        // 1. إرسال رمز التحقق (OTP) عند نسيان كلمة المرور (Forgot Password)
        // =========================================================================
        public async Task<(bool Success, string Message, string? MaskedEmail)> SendForgotPasswordOtpAsync(string emailOrAccountNumber)
        {
            if (string.IsNullOrWhiteSpace(emailOrAccountNumber))
                return (false, "البريد الإلكتروني أو رقم الحساب مطلوب.", null);

            string cleanInput = emailOrAccountNumber.Trim().ToLower();

            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
                u => (u.Email != null && u.Email.ToLower() == cleanInput) || (u.AccountNumber != null && u.AccountNumber.ToLower() == cleanInput),
                u => u.Person,
                u => u.UserRole
            );

            var user = users.FirstOrDefault();

            if (user == null || user.Person == null || !user.Person.IsActive)
            {
                return (false, "البريد الإلكتروني أو رقم الحساب المدخل غير مسجل لدينا أو غير نشط.", null);
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return (false, "لا يوجد بريد إلكتروني مسجل لهذا الحساب للتواصل. يُرجى مراجعة إدارة المدرسة.", null);
            }

            string userEmail = user.Email.Trim().ToLower();

            if (_otpService.IsBlocked($"RESET_{userEmail}"))
            {
                return (false, "لقد تم حظر طلبات التحقق لهذا الحساب مؤقتاً لتكرار المحاولات الخاطئة. يُرجى المحاولة بعد 5 دقائق.", null);
            }

            // توليد رمز الـ OTP وتخزينه في الكاش لمدة 10 دقائق
            string otp = _otpService.StoreResetPasswordOtp(userEmail);

            string recipientName = $"{user.Person.FirstName} {user.Person.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                recipientName = "عزيزنا المستخدم";
            }

            // إرسال الـ OTP عبر البريد الإلكتروني
            await _emailService.SendForgotPasswordOtpAsync(userEmail, recipientName, otp);

            string masked = MaskEmail(userEmail);
            return (true, $"تم إرسال رمز التحقق إلى بريدك الإلكتروني المسجل ({masked}) بنجاح (صالح لمدة 10 دقائق).", masked);
        }

        // =========================================================================
        // 2. التحقق من صحة رمز الـ OTP لإعادة تعيين كلمة المرور (Verify Reset OTP)
        // =========================================================================
        public Task<(bool Success, string Message)> VerifyResetOtpAsync(VerifyResetOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
            {
                return Task.FromResult((false, "البريد الإلكتروني ورمز التحقق مطلوبان."));
            }

            string cleanEmail = dto.Email.Trim().ToLower();
            bool isValid = _otpService.ValidateResetPasswordOtp(cleanEmail, dto.Otp.Trim(), out string? errorMessage);

            if (!isValid)
            {
                return Task.FromResult((false, errorMessage ?? "رمز التحقق غير صحيح أو منتهي الصلاحية."));
            }

            return Task.FromResult((true, "تم التحقق من الرمز بنجاح! يمكنك الآن إدخال كلمة المرور الجديدة."));
        }

        // =========================================================================
        // 3. تعيين كلمة المرور الجديدة وتحديثها في قاعدة البيانات (Reset Password)
        // =========================================================================
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto == null)
                return (false, "بيانات إعادة تعيين كلمة المرور مطلوبة.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return (false, "يجب ألا تقل كلمة المرور عن 6 خانات.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return (false, "كلمتا المرور غير متطابقتين.");

            string cleanEmail = dto.Email.Trim().ToLower();

            // التحقق من أن جلسة إعادة التعيين موثقة عبر التحقق المسبق من الـ OTP
            bool isVerified = _otpService.IsResetOtpVerified(cleanEmail);

            // في حال لم يتم استدعاء خطوة verify مسبقاً، نتحقق من الـ OTP مباشرة
            //if (!isVerified)
            //{
            //    bool otpValid = _otpService.ValidateResetPasswordOtp(cleanEmail, dto.Otp.Trim(), out string? errMsg);
            //    if (!otpValid)
            //    {
            //        return (false, errMsg ?? "يجب التحقق من رمز التحقق أولاً أو انتهت صلاحية الجلسة.");
            //    }
            //}

            // جلب المستخدم وتحديث كلمة المرور
            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
                u => u.Email != null && u.Email.ToLower() == cleanEmail,
                u => u.Person
            );

            var user = users.FirstOrDefault();
            if (user == null)
            {
                return (false, "المستخدم غير موجود بالنظام.");
            }

            // تشفير كلمة المرور وتخزينها
            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword.Trim());
            _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            // تنظيف الكاش من بيانات الـ OTP
            _otpService.ClearResetPassword(cleanEmail);

            // إلغاء أي جلسات دخول سابقة للحساب لتعزيز الأمان
            var existingTokens = await _refreshTokenRepo.GetAllWithIncludeAndFilterAsync(
                t => t.UserId == user.UserId && t.RevokedOn == null
            );
            foreach (var token in existingTokens)
            {
                token.RevokedOn = DateTime.UtcNow;
                _refreshTokenRepo.UpdateAsync(token);
            }
            await _refreshTokenRepo.SaveChangesAsync();

            return (true, "تم تغيير كلمة المرور بنجاح وحفظها! يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة.");
        }

    }

}

