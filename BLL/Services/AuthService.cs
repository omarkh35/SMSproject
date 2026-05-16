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


        public AuthService(
            IBaseRepositories<User> userRepo,
            IBaseRepositories<UserRefreshToken> refreshTokenRepo,
            IJwtService jwtService)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            
            _jwtService = jwtService;

        }


        public async Task<TokenResponseDto?> LoginAsync(LoginRequestDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.AccountNumber))
                return null;

            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(
        u => u.AccountNumber == loginDto.AccountNumber,
        u => u.Person,
        u => u.UserRole
    );


            var user = users.FirstOrDefault();


            if (user == null)
                return null;

            //var passCheck = _passwordHasher.VerifyHashedPassword(user, user.PassHash, loginDto.Password);

            //if (passCheck == PasswordVerificationResult.Failed)
            //    return null;
            var passCheck = (loginDto.Password == user.HashPassword);
            if (!passCheck)
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
                User = new EntitiesDTOS.User.UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.UserRole.RoleName ?? "No Role",
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
            if (user == null)
                return null;

          
            var tokenResponse = _jwtService.GenerateToken(user);

            
            currentRefreshToken.RevokedOn = DateTime.UtcNow;
            currentRefreshToken.ReplacedByToken = tokenResponse.RefreshToken;
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
                User = new EntitiesDTOS.User.UserDto
                {
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.UserRole.RoleName ?? "No Role",
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

    }
}
