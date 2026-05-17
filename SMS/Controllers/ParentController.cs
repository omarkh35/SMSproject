using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace SMS.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParentController : ControllerBase
    {
        private readonly IParentService _parentService;

        public ParentController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpGet("MyChildren")]
        public async Task<IActionResult> GetMyChildren()
        {

            var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "THere is a problem with the Identity" });
            }

            var children = await _parentService.GetMyChildrenAsync(parentPersonId);

            return Ok(children);
        }

        [HttpGet("ParentHomePage")]
        public async Task<IActionResult> GetParentHomePage()
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity error." });
            }

            var dashboardData = await _parentService.GetParentDashboardAsync(parentPersonId);
            return Ok(dashboardData);
        }

        [HttpGet("children/{studentId}/bag")]
        public async Task<IActionResult> GetStudentBagDetails(int studentId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity error." });
            }

            var details = await _parentService.GetStudentBagDetailsAsync(parentPersonId, studentId);

            if (details == null)
            {
                return Forbid();
            }

            return Ok(details);
        }

        [HttpGet("student/{studentId}/weeklyschedule")]
        public async Task<IActionResult> GetStudentWeeklySchedule(int studentId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity claim parsing fail" });
            }

            string hostUrl = $"{Request.Scheme}://{Request.Host}";

            var schedule = await _parentService.GetStudentWeeklyScheduleAsync(parentPersonId, studentId, hostUrl);

            if (schedule == null)
            {
                return Forbid();
            }

            return Ok(schedule);
        }


        [HttpGet("student/{studentId}/exam-schedule")]
        public async Task<IActionResult> GetStudentExamSchedule(int studentId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity context is missing." });
            }

            string hostUrl = $"{Request.Scheme}://{Request.Host}";

            var examSchedule = await _parentService.GetStudentExamScheduleAsync(parentPersonId, studentId, hostUrl);

            if (examSchedule == null)
            {
                return Forbid();
            }

            return Ok(examSchedule);
        }


        [HttpGet("student/{studentId}/academic-summary")]
        public async Task<IActionResult> GetStudentAcademicSummary(int studentId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity claim context parsing failed." });
            }


            var summary = await _parentService.GetStudentAcademicSummaryAsync(parentPersonId, studentId);

            if (summary == null)
            {
                return Forbid();
            }

            return Ok(summary);
        }

        [HttpGet("student/{studentId}/profile")]
        public async Task<IActionResult> GetStudentProfile(int studentId)
        {
            var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity context is missing." });
            }

            string hostUrl = $"{Request.Scheme}://{Request.Host}";


            var profile = await _parentService.GetStudentProfileAsync(parentPersonId, studentId, hostUrl);

            if (profile == null)
            {
                return Forbid();
            }

            return Ok(profile);
        }


        [HttpGet("student/{studentId}/attendance")]
        public async Task<IActionResult> GetStudentAttendance(int studentId, [FromQuery] int? year, [FromQuery] int? month)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity context is missing or corrupted." });
            }

            int filterYear = year ?? DateTime.UtcNow.Year;
            int filterMonth = month ?? DateTime.UtcNow.Month;

            var attendanceData = await _parentService.GetStudentAttendanceCalendarAsync(parentPersonId, studentId, filterYear, filterMonth);

            if (attendanceData == null)
            {
                return Forbid(); 
            }

            return Ok(attendanceData);
        }



        [HttpGet("student/{studentId}/subject/{subjectId}/detailes")]
        public async Task<IActionResult> GetSubjectDetailedReport(int studentId, int subjectId)
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
            {
                return Unauthorized(new { message = "User identity context is missing." });
            }

            var reportCard = await _parentService.GetSubjectDetailedReportAsync(parentPersonId, studentId, subjectId);

            if (reportCard == null)
            {
                return Forbid(); 
            }

            return Ok(reportCard);
        }





    }


}

