using BLL.EntitiesDTOS.Auth;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SMS.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized("Invalid credentials or user is banned.");

            return Ok(result);
        }





        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            return Ok(result);
        }

        //منحطها بعد ما نحط ال Baerer
        //[Authorize] 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LogoutAsync(request);

            if (!result)
            {
                return BadRequest(new { message = "Invalid or already revoked refresh token." });
            }

            return Ok(new { message = "Logged out successfuly. Token has been revoked." });
        }


    }
}

