using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BLL.EntitiesDTOS.Auth;
using System.Security.Cryptography;

namespace SMS.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            //جيب رقم الزلمة 
            var user = StudentDataSimulation.StudentsList
                .FirstOrDefault(s => s.Email == request.Email);


            //اذا مو موجود
            if (user == null)
                return Unauthorized("Invalid credentials");


            //تاكد اذا Hash الباسوود صح 
            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);


           
            if (!isValidPassword)
                return Unauthorized("Invalid credentials");


            // Step 3: Create claims that represent the authenticated user's identity.
            // These claims will be embedded inside the JWT.
            var claims = new[]
            {
                
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),


               
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),


                
                new Claim(ClaimTypes.Role, user.Role)
            };


            
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));


            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            
            var token = new JwtSecurityToken(
                issuer: "SchoolApi",
                audience: "SchoolApiUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

           
            var refreshToken = GenerateRefreshToken();

            
            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;

            
            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }



        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var user = StudentDataSimulation.StudentsList
                .FirstOrDefault(s => s.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid refresh request");

            if (user.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh token is revoked");

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Unauthorized("Invalid refresh token");

            // Issue NEW access token (same claims & signing settings as login)
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_SECRET_KEY_123456"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: "SchoolApi",
                audience: "SchoolApiUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Rotation: replace refresh token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            var user = StudentDataSimulation.StudentsList
                .FirstOrDefault(s => s.Email == request.Email);

            if (user == null)
                return Ok(); 

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            return Ok("Logged out successfully");
        }


    }
}

