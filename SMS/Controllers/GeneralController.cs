using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GeneralController : ControllerBase
    {
        private readonly ISchoolSettingService _schoolSettingService;

        public GeneralController(ISchoolSettingService schoolSettingService)
        {
            _schoolSettingService = schoolSettingService;
        }

        [AllowAnonymous]
        [HttpGet("school-info")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetSchoolInfo()
        {
            var result = await _schoolSettingService.GetSchoolInfoAsync();
            return Ok(result);
        }
    }
}
