using BLL.EntitiesDTOS.Teacher;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace SMS.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService TeacherService)
        {
            _teacherService = TeacherService;
        }


        [HttpGet("HomeScreen")]
        public async Task<IActionResult> GetDashboard()
        {

            //هون منجيب اي دي مشان نجيب معلوماتو
            var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
            {
                return Unauthorized(new { message = "User identity claim context missing." });
            }

            var dashboard = await _teacherService.GetTeacherDashboardAsync(teacherPersonId);
            if (dashboard == null) 
                return NotFound(new { message = "Teacher personal record context not found." });

            return Ok(dashboard);
        }



        [HttpGet("profile")]
        public async Task<IActionResult> GetDetailedProfile()
        {


            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            
            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
            {
                return Unauthorized(new { message = "User identity claim context missing." });
            }

            var profile = await _teacherService.GetTeacherDetailedProfileAsync(teacherPersonId);

            if (profile == null)
            {
                return NotFound(new { message = "Detailed teacher profile not found." });
            }

            return Ok(profile);
        }



        [HttpGet("classes")]
        public async Task<IActionResult> GetMyClasses()
        {
            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
            {
                return Unauthorized(new { message = "User identity context missing." });
            }

            var chips = await _teacherService.GetTeacherClassesChipsAsync(teacherPersonId);
            return Ok(chips);
        }

        [HttpGet("classes/{classRoomId}/students")]
        public async Task<IActionResult> GetClassStudents(int classRoomId)
        {
            var students = await _teacherService.GetStudentsInClassAsync(classRoomId);

            if (students == null || !students.Any())
            {
                return NotFound(new { message = "No students are currently enrolled in this classroom." });
            }

            return Ok(students);
        }


        [HttpPost("grades/save")]
        public async Task<IActionResult> SaveGrades([FromBody] SaveGradesBulkDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isSaved = await _teacherService.SaveStudentGradesAsync(request);

            if (!isSaved)
            {
                return BadRequest(new { message = "Failed to save grades. Please verify the request input parameters data." });
            }

            return Ok(new { message = "Student grades have been uploaded successfully and are pending administrative review." });
        }



        [HttpPost("attendance/save")]
        public async Task<IActionResult> SaveAttendance([FromBody] SaveAttendanceBulkDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var statusResult = await _teacherService.SaveBulkAttendanceAsync(request);

            if (!statusResult)
            {
                return BadRequest(new { message = "Failed to store attendance log data matrix payload." });
            }

            return Ok(new { message = "Classroom student attendance updated successfully." });
        }




        [HttpGet("classes/{classRoomId}/students-search")]
        public async Task<IActionResult> GetClassStudentsWithFilter(int classRoomId, [FromQuery] string? search)
        {
            var students = await _teacherService.GetStudentsInClassWithSearchAsync(classRoomId, search);
            return Ok(students);
        }


        [HttpPost("notes/save")]
        public async Task<IActionResult> SaveNoteToParent([FromBody] SaveStudentNoteDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
            {
                return Unauthorized(new { message = "User identity context missing." });
            }

            var result = await _teacherService.SaveStudentNoteAsync(teacherPersonId, request);

            if (!result)
            {
                return BadRequest(new { message = "Failed to submit note. Verify parameter requirements." });
            }

            return Ok(new { message = "Note successfully saved and delivered to the parent application feed." });
        }


       
        [HttpPost("DailyLesson")]
        public async Task<IActionResult> SaveDailyLesson([FromBody] SaveDailyLessonDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
                {
                    return Unauthorized(new { message = "Invalid teacher identity context." });
                }

                var result = await _teacherService.SaveDailyLessonAsync(teacherPersonId, request);

                if (!result)
                    return BadRequest(new { message = "Could not save lesson details. Check your inputs." });

                return Ok(new { message = "Lesson execution summary saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred.", error = ex.Message });
            }
        }

        
        [HttpPost("assign/homework")]
        public async Task<IActionResult> AssignHomework([FromBody] SaveHomeworkDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
                {
                    return Unauthorized(new { message = "Corrupted user identity context." });
                }

                var result = await _teacherService.CreateHomeworkAssignmentAsync(teacherPersonId, request);

                if (!result)
                    return BadRequest(new { message = "Failed to store the homework entity configuration." });

                return Ok(new { message = "Homework assignment created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A server error occurred processing request.", error = ex.Message });
            }
        }



    }




}

