using BLL.EntitiesDTOS.Accountant;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Accountant")]
    public class AccountantController : ControllerBase
    {
        private readonly IAccountantService _accountantService;

        public AccountantController(IAccountantService accountantService)
        {
            _accountantService = accountantService;
        }

        [HttpGet("students-dashboard")]
        public async Task<IActionResult> GetStudentsDashboard([FromQuery] string? searchName, [FromQuery] int? classRoomId, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var dashboardGridResult = await _accountantService.GetMainDashboardGridAsync(searchName, classRoomId, page);
            return Ok(dashboardGridResult);
        }

        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _accountantService.RegisterNewStudentAsync(dto);
            return success ? Ok(new { message = "Student registered successfully." })
                           : BadRequest("Failed to complete student registration transaction.");
        }

        [HttpGet("student-details/{studentId}")]
        public async Task<IActionResult> GetStudentDetails(int studentId)
        {
            var details = await _accountantService.GetStudentDetailsForFormAsync(studentId);
            if (details == null) return NotFound($"No student found with ID {studentId}");

            return Ok(details);
        }

        [HttpPut("student-details/{studentId}")]
        public async Task<IActionResult> UpdateStudentDetails(int studentId, [FromBody] StudentRegistrationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _accountantService.UpdateStudentRegistrationAsync(studentId, dto);
            return success ? Ok(new { message = "Student profile updated successfully." })
                           : BadRequest("Failed to process student updates transaction parameters.");
        }

        [HttpDelete("student-record/{studentId}")]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            var success = await _accountantService.DeleteStudentRecordWorkflowAsync(studentId);
            return success ? Ok(new { message = "Student academic records removed successfully." })
                           : BadRequest("Failed to execute student deletion workflow safe-checks.");
        }
    }
}
