using BLL.EntitiesDTOS.Supervisor;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Supervisor")]
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
            if (result == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have administrative oversight over this classroom." });

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
            if (result == null) return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have administrative oversight over this classroom." });

            return Ok(result);
        }

        //[HttpPost("save-attendance-sheet")]
        //public async Task<IActionResult> SaveDailyAttendanceSheet([FromBody] SaveAttendanceSheetDto dto)
        //{
        //    var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        //    var success = await _supervisorService.SaveAttendanceSheetWorkflowAsync(personId, dto);
        //    return success ? Ok(new { message = "Attendance sheet synchronized successfully." })
        //                   : BadRequest("Failed to process attendance grid save.");
        //}

        [HttpPost("save-students-attendance")]
        public async Task<IActionResult> SaveStudentsAttendance([FromBody] SaveStudentAttendanceDto dto)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.SaveStudentAttendanceWorkflowAsync(personId, dto);
            return success ? Ok(new { message = "Student attendance grid saved successfully." })
                           : StatusCode(StatusCodes.Status403Forbidden, new { message = "Failed to process student save or you lack classroom oversight." });
        }

        [HttpPost("save-teachers-attendance")]
        public async Task<IActionResult> SaveTeachersAttendance([FromBody] SaveTeacherAttendanceDto dto)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.SaveTeacherAttendanceWorkflowAsync(personId, dto);
            return success ? Ok(new { message = "Teacher attendance log synchronized successfully." })
                           : BadRequest("Failed to process teacher attendance save.");
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

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

        [HttpGet("chats")]
        public async Task<IActionResult> GetChatThreads()
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var threads = await _supervisorService.GetSupervisorChatThreadsAsync(personId);
            return Ok(threads);
        }

        [HttpGet("chat-history/{chatRoomId}")]
        public async Task<IActionResult> GetChatHistory(int chatRoomId)
        {
            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var history = await _supervisorService.GetChatHistoryAsync(personId, chatRoomId);
            return Ok(history);
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MessageContent))
                return BadRequest("Message content cannot be empty.");

            var personId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var success = await _supervisorService.SendMessageAsync(personId, dto);
            return success ? Ok(new { message = "Message dispatched and thread updated successfully." })
                           : BadRequest("Failed to process message transmission.");
        }


    }
}
