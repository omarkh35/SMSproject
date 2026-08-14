using BLL.EntitiesDTOS.Admin;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAuthController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;

        public AdminAuthController(IAdminAuthService adminAuthService)
        {
            _adminAuthService = adminAuthService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminAuthService.AdminLoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "عذراً، اسم المستخدم أو كلمة المرور غير صالحة." });

            return Ok(result);
        }
    }
}
