using BLL.EntitiesDTOS.Supervisor;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupervisorController : Controller
    {

        private readonly ISupervisorService _supervisorService;

        public SupervisorController(ISupervisorService supervisorService) 
        { 
            _supervisorService = supervisorService; 
        }



        [HttpGet("main-dashboard")]
        public async Task<IActionResult> GetDashboardCoreMetrics()
        {
            // Extract supervisor's secure identity PersonID claim from JWT token context
            var supervisorPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.GetMainDashboardAsync(supervisorPersonId);
            return Ok(result);
        }

        // ENDPOINT 2: Dropdown Value Change Event Listener Target
        [HttpGet("classroom/{classRoomId}")]
        public async Task<IActionResult> GetClassroomRollCall(int classRoomId)
        {
            var supervisorPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.GetClassroomRollCallAsync(supervisorPersonId, classRoomId);
            if (result == null) return Forbid("You do not have administrative oversight over this classroom.");

            return Ok(result);
        }


        [HttpGet("tasks")]
        public async Task<IActionResult> GetTodayTasks()
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var result = await _supervisorService.GetTodayTasksAsync(personId);
            return Ok(result);
        }

        [HttpPost("tasks")]
        public async Task<IActionResult> AddTask([FromBody] CreateTaskDto dto)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var success = await _supervisorService.CreateTaskAsync(personId, dto);
            return success ? Ok() : BadRequest();
        }

        [HttpPut("tasks/{taskId}/toggle")]
        public async Task<IActionResult> ToggleTask(long taskId)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var success = await _supervisorService.ToggleTaskAsync(personId, taskId);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(long taskId)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var success = await _supervisorService.DeleteTaskAsync(personId, taskId);
            return success ? Ok() : NotFound();
        }

        [HttpGet("classroom/{classRoomId}/attendance-sheet")]
        public async Task<IActionResult> GetDailyAttendanceSheet(int classRoomId)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.LoadAttendanceSheetAsync(personId, classRoomId);
            if (result == null) return Forbid("You do not manage this classroom.");

            return Ok(result);
        }

        [HttpPost("save-attendance-sheet")]
        public async Task<IActionResult> SaveDailyAttendanceSheet([FromBody] SaveAttendanceSheetDto dto)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.SaveAttendanceSheetWorkflowAsync(personId, dto);
            return success ? Ok(new { message = "Attendance sheet synchronized successfully." })
                           : BadRequest("Failed to process attendance grid save.");
        }

        [HttpGet("announcements-panel")]
        public async Task<IActionResult> GetAnnouncementsPanel()
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.LoadAnnouncementsPanelAsync(personId);
            return Ok(result);
        }

        [HttpPost("announcements")]
        public async Task<IActionResult> PublishAnnouncement([FromBody] CreateAnnouncementRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.PublishAnnouncementAsync(personId, dto);
            return success ? Ok(new { message = "Announcement published successfully." }) : BadRequest();
        }

        [HttpDelete("announcements/{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.DeleteAnnouncementAsync(personId, id);
            return success ? Ok(new { message = "Announcement removed successfully." }) : NotFound();
        }

        [HttpGet("classrooms-directory")]
        public async Task<IActionResult> GetClassroomsDirectory()
        {
            // Secure identity extraction from JWT Token claims
            var supervisorPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.GetSupervisedClassroomsDirectoryAsync(supervisorPersonId);
            return Ok(result);
        }

        [HttpGet("students-directory")]
        public async Task<IActionResult> GetStudentsDirectory([FromQuery] int? classRoomId, [FromQuery] string? search)
        {
            var supervisorPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.GetMyStudentsDirectoryAsync(supervisorPersonId, classRoomId, search ?? string.Empty);
            return Ok(result);
        }

        [HttpGet("students/{studentId}/details")]
        public async Task<IActionResult> GetStudentProfileDetails(int studentId, [FromQuery] int month, [FromQuery] int year)
        {
            // Fall back to current system calendar metrics if parameters are omitted by mobile UI
            if (month < 1 || month > 12) month = DateTime.UtcNow.Month;
            if (year < 2000) year = DateTime.UtcNow.Year;

            var result = await _supervisorService.GetStudentDetailedProfileAsync(studentId, month, year);
            if (result == null) return NotFound("Student profile record not found.");

            return Ok(result);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachersSidebar([FromQuery] string? search)
        {
            var supervisorPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _supervisorService.GetSupervisedTeachersSidebarAsync(supervisorPersonId, search ?? string.Empty);
            return Ok(result);
        }

        [HttpGet("teachers/{teacherId}/details")]
        public async Task<IActionResult> GetTeacherPaneDetails(int teacherId, [FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12) 
                month = DateTime.UtcNow.Month;
            if (year < 2000) 
                year = DateTime.UtcNow.Year;

            var result = await _supervisorService.GetTeacherPaneDetailsAsync(teacherId, month, year);
            if (result == null) return NotFound("Teacher record could not be resolved.");

            return Ok(result);
        }

    }
}
