using BLL.EntitiesDTOS.Admin;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IBaseRepositories<SuperAdmin> _adminRepo;
        private readonly IBaseRepositories<UserRefreshToken> _refreshTokenRepo;
        private readonly IJwtService _jwtService;

        public AdminAuthService(
            IBaseRepositories<SuperAdmin> adminRepo,
            IBaseRepositories<UserRefreshToken> refreshTokenRepo,
            IJwtService jwtService)
        {
            _adminRepo = adminRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtService = jwtService;
        }

        public async Task<AdminTokenResponseDto?> AdminLoginAsync(AdminLoginRequestDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                return null;

            // 1. جلب حسابات المدراء ومطابقة الاسم الصافي
            var allAdmins = await _adminRepo.GetAllAsync();
            var adminAccount = allAdmins.FirstOrDefault(a => a.Username.Trim() == loginDto.Username.Trim());

            if (adminAccount == null) return null;

            // 2. المطابقة النصية المباشرة لكلمة السر الصافية (Plain Text) بناءً على طلبك
            bool isPasswordValid = (loginDto.Password == adminAccount.StaticPassword);
            if (!isPasswordValid) return null;

            // 3. استدعاء الدالة الجديدة المضافة وتمرير نصوص المدير مباشرة
            var tokenResult = _jwtService.GenerateToken(adminAccount.Username, "SuperAdmin");

            

            return new AdminTokenResponseDto
            {
                Username = adminAccount.Username,
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken
            };
        }

    }
}
