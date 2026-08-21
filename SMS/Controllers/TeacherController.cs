using BLL.EntitiesDTOS.Teacher;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace SMS.Controllers
{

    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]

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

            try
            {
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب لوحة معلومات المعلم.",
                    details = ex.Message
                });
            }
        }



        [HttpGet("profile")]
        public async Task<IActionResult> GetDetailedProfile()
        {

            try
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب الملف الشخصي للمعلم.",
                    details = ex.Message
                });
            }
        }



        [HttpGet("classes")]
        public async Task<IActionResult> GetMyClasses()
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
                {
                    return Unauthorized(new { message = "User identity context missing." });
                }

                var chips = await _teacherService.GetTeacherClassesChipsAsync(teacherPersonId);
                return Ok(chips);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب الشعب الصفية للمعلم.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("classes/{classRoomId}/students")]
        public async Task<IActionResult> GetClassStudents(int classRoomId)
        {
            try
            {
                var students = await _teacherService.GetStudentsInClassAsync(classRoomId);

                if (students == null || !students.Any())
                {
                    return NotFound(new { message = "No students are currently enrolled in this classroom." });
                }

                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب قائمة طلاب الشعبة.",
                    details = ex.Message
                });
            }
        }


        [HttpPost("grades/save")]
        public async Task<IActionResult> SaveGrades([FromBody] SaveGradesBulkDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var isSaved = await _teacherService.SaveStudentGradesAsync(request);

                if (!isSaved)
                {
                    return BadRequest(new { message = "Failed to save grades. Please verify the request input parameters data." });
                }

                return Ok(new { message = "Student grades have been uploaded successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء حفظ درجات الطلاب.",
                    details = ex.Message
                });
            }
        }



        [HttpPost("attendance/save")]
        public async Task<IActionResult> SaveAttendance([FromBody] SaveAttendanceBulkDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var statusResult = await _teacherService.SaveBulkAttendanceAsync(request);

                if (!statusResult)
                {
                    return BadRequest(new { message = "Failed to store attendance log data matrix payload." });
                }

                return Ok(new { message = "Classroom student attendance updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء حفظ حضور وغياب الطلاب.",
                    details = ex.Message
                });
            }
        }




        [HttpGet("classes/{classRoomId}/students-search")]
        public async Task<IActionResult> GetClassStudentsWithFilter(int classRoomId, [FromQuery] string? search)
        {
            try
            {
                var students = await _teacherService.GetStudentsInClassWithSearchAsync(classRoomId, search);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء البحث في طلاب الشعبة.",
                    details = ex.Message
                });
            }
        }


        [HttpPost("notes/save")]
        public async Task<IActionResult> SaveNoteToParent([FromBody] SaveStudentNoteDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء إرسال الملاحظة لولي الأمر.",
                    details = ex.Message
                });
            }

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
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Teacher")] 
        public async Task<IActionResult> AssignHomework([FromForm] SaveHomeworkDto request) 
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


        [HttpGet("weekly-schedule")]
        public async Task<IActionResult> GetWeeklySchedule()
        {

            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
                {
                    return Unauthorized(new { message = "User identity claim context missing." });
                }

                string hostUrl = $"{Request.Scheme}://{Request.Host}";
                var schedule = await _teacherService.GetTeacherWeeklyScheduleAsync(teacherPersonId, hostUrl);

                if (schedule == null)
                {
                    return NotFound(new { message = "No weekly schedule found for this teacher." });
                }

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب جدول الدوام الأسبوعي للمعلم.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("exam-schedules")]
        public async Task<IActionResult> GetExamSchedules()
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int teacherPersonId))
                {
                    return Unauthorized(new { message = "User identity claim context missing." });
                }

                string hostUrl = $"{Request.Scheme}://{Request.Host}";
                var examSchedules = await _teacherService.GetTeacherExamSchedulesAsync(teacherPersonId, hostUrl);

                return Ok(examSchedules);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب جدول الامتحانات للمعلم.",
                    details = ex.Message
                });
            }
        }


    }




}

