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

            try
            {
                var success = await _accountantService.RegisterNewStudentAsync(dto);
                return success ? Ok(new { message = "تم تسجيل الطالب بنجاح وربطه بحساب العائلة." })
                               : BadRequest("فشلت عملية حفظ بيانات الطالب في النظام.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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

        [HttpGet("parent-accounts")]
        public async Task<IActionResult> GetParentAccounts([FromQuery] string? searchQuery, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _accountantService.GetParentAccountsGridAsync(searchQuery, page);
            return Ok(result);
        }

        [HttpGet("installment-tracking")]
        public async Task<IActionResult> GetInstallmentTracking(
    [FromQuery] string? status = "All",
    [FromQuery] string? searchName = null,
    [FromQuery] int? classRoomId = null,
    [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _accountantService.GetInstallmentTrackingGridAsync(status, searchName, classRoomId, page);
            return Ok(result);
        }


        [HttpGet("student-payments/{studentId}/details")]
        public async Task<IActionResult> GetStudentPaymentDetails(int studentId)
        {
            var result = await _accountantService.GetStudentPaymentDetailsAsync(studentId);
            if (result == null)
                return NotFound(new { message = $"No enrollment or academic fee mapping record found for Student ID {studentId}." });

            return Ok(result);
        }


        [HttpGet("educational-staff-salaries")]
        public async Task<IActionResult> GetEducationalStaffSalaries()
        {
            var result = await _accountantService.GetEducationalStaffSalariesAsync();
            return Ok(result);
        }
    }
}
