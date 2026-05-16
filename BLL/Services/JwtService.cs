using BLL.EntitiesDTOS.Auth;
using Microsoft.IdentityModel.Tokens;
using DAL.Entities;
using BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class JwtService : IJwtService
    {

        //public TokenResponseDto GenerateToken(User user)
        //{
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        //        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
        //        new Claim(ClaimTypes.Name, $"{user.Person.FirstName} {user.Person.LastName}"),
        //        //هون يمكن لازم RoleID بعدين منشوف
        //        new Claim(ClaimTypes.Role, user.UserRole.RoleName)
        //    };

        //    var key = new SymmetricSecurityKey(
        //         Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



        //    var token = new JwtSecurityToken(
        //         issuer: "SchoolApi",
        //         audience: "SchoolApiUsers",
        //         claims: claims,
        //         expires: DateTime.Now.AddMinutes(30),
        //         signingCredentials: creds
        //     );

        //    var accessToken = new JwtSecurityTokenHandler().WriteToken(token);


        //    var refreshToken = GenerateRefreshToken();


        //    UserRefreshToken userRefresh = new UserRefreshToken();

        //    userRefresh.UserId = user.UserId;

        //    userRefresh.ReplacedByToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        //    userRefresh.ExpiresOn = DateTime.UtcNow.AddDays(7);
        //    userRefresh.RevokedOn = null;

        //    return new TokenResponseDto
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken,

        //    };
        //}

        public TokenResponseDto GenerateToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
        new Claim(ClaimTypes.Name, $"{user.Person?.FirstName} {user.Person?.LastName}"),
        new Claim(ClaimTypes.Role, user.UserRole?.RoleName ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                 issuer: "SchoolApi",
                 audience: "SchoolApiUsers",
                 claims: claims,
                 expires: DateTime.UtcNow.AddMinutes(30),
                 signingCredentials: creds
             );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();


            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }


    }
}
