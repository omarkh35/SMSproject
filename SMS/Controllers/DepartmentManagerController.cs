using BLL.EntitiesDTOS.DepartmentManager;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentManagerController : ControllerBase
    {
        private readonly IDepartmentManagerService _deptService;

        public DepartmentManagerController(IDepartmentManagerService deptService)
        {
            _deptService = deptService;
        }

        [HttpGet("classrooms")]
        public async Task<IActionResult> GetAllClassRooms()
        {
            var result = await _deptService.GetAllClassRoomsAsync();
            return Ok(result);
        }

        [HttpGet("classroom/{id}")]
        public async Task<IActionResult> GetClassRoomById(int id)
        {
            var result = await _deptService.GetClassRoomByIdAsync(id);
            if (result == null) return NotFound("الصف غير موجود");
            return Ok(result);
        }

        //[HttpPost("classroom")]
        //public async Task<IActionResult> CreateClassRoom([FromBody] ClassRoomCreateDto dto)
        //{
        //    if (!ModelState.IsValid) return BadRequest(ModelState);
        //    var result = await _deptService.CreateClassRoomAsync(dto);
        //    return Ok(result);
        //}

        [HttpPut("classroom/{id}")]
        public async Task<IActionResult> UpdateClassRoom(int id, [FromBody] ClassRoomUpdateDto dto)
        {
            var success = await _deptService.UpdateClassRoomAsync(id, dto);
            if (!success) return NotFound("فشل التحديث، الصف غير موجود");
            return Ok(new { message = "تم تحديث الصف بنجاح" });
        }

        [HttpDelete("classroom/{id}")]
        public async Task<IActionResult> DeleteClassRoom(int id)
        {
            var success = await _deptService.DeleteClassRoomAsync(id);
            if (success == false) return NotFound("الصف غير موجود");
            return Ok(new { message = "تم الحذف بنجاح " });
        }

        [HttpPost("assign-student-to-class")]
        public async Task<IActionResult> AssignStudentToClass([FromBody] StudentToClassDto dto)
        {
            var success = await _deptService.AssignStudentToClassAsync(dto);
            if (!success) return BadRequest("فشل الربط، يرجى التأكد من البيانات");
            return Ok(new { message = "تم ربط الطالب بالصف بنجاح" });
        }

        [HttpDelete("remove-student-from-class")]
        public async Task<IActionResult> RemoveStudentFromClass(int studentId, int classRoomId)
        {
            var success = await _deptService.RemoveStudentFromClassAsync(studentId, classRoomId);
            if (!success) return NotFound("الطالب غير موجود في الصف المحدد");
            return Ok(new { message = "تمت إزالة الطالب من الصف بنجاح" });
        }



        [HttpPost("assign-teacher-to-class")]
        public async Task<IActionResult> AssignTeacherToClass([FromBody] TeacherToClassDto dto)
        {
            var success = await _deptService.AssignTeacherToClassAsync(dto);
            if (!success) return BadRequest("فشل ربط المدرس");
            return Ok(new { message = "تم ربط المدرس بالصف والمادة بنجاح" });
        }

        [HttpPut("update-teacher-assignment")]
        public async Task<IActionResult> UpdateTeacherAssignment([FromBody] TeacherToClassDto dto)
        {
            var success = await _deptService.UpdateTeacherAssignmentAsync(dto);
            if (!success) return NotFound("خطأ في البيانات");
            return Ok(new { message = "تم تحديث تعيين المدرس بنجاح " });
        }

        [HttpDelete("remove-teacher-from-class")]
        public async Task<IActionResult> RemoveTeacherFromClass(int teacherId, int classRoomId)
        {
            var success = await _deptService.RemoveTeacherFromClassAsync(teacherId, classRoomId);
            if (!success) return NotFound("التعيين غير موجود");
            return Ok(new { message = "تمت إزالة المدرس من الصف بنجاح" });
        }

        [HttpPost("assign-supervisor-to-teacher")]
        public async Task<IActionResult> AssignSupervisorToTeacher([FromBody] TeacherSupervisorDto dto)
        {
            var success = await _deptService.AssignSupervisorToTeacherAsync(dto);
            if (!success) return BadRequest("فشل الربط");
            return Ok(new { message = "تم ربط الموجه بالمدرس بنجاح" });
        }

        [HttpDelete("remove-supervisor-from-teacher")]
        public async Task<IActionResult> RemoveSupervisorFromTeacher(int supervisorId, int teacherId)
        {
            var success = await _deptService.RemoveSupervisorFromTeacherAsync(supervisorId, teacherId);
            if (!success) return NotFound("العلاقة غير موجودة");
            return Ok(new { message = "تمت إزالة الموجه من إشراف المدرس" });
        }


        ////////////////////////////////////////////////////////
       

        [HttpGet("students")]
        public async Task<IActionResult> GetStudentDirectoryGrid([FromQuery] string? searchName, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

           
            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _deptService.GetStudentDirectoryDashboardAsync(managerPersonId, searchName, page);
            return Ok(result);
        }


        [HttpGet("supervisors")]
        public async Task<IActionResult> GetSupervisorsDashboardSummary()
        {
            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _deptService.GetSupervisorsManagementDashboardAsync(managerPersonId);
            return Ok(result);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachersManagementGrid()
        {
            var result = await _deptService.GetTeachersManagementDashboardAsync();
            return Ok(result);
        }

        [HttpPost("register-supervisors")]
        public async Task<IActionResult> RegisterNewSupervisor([FromBody] CreateSupervisorDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var generatedAccount = await _deptService.RegisterSupervisorWorkflowAsync(managerPersonId, dto);

            if (generatedAccount != null)
            {
                return Ok(new
                {
                    message = "تم تسجيل الموجه بنجاح.",
                    accountNumber = generatedAccount
                });
            }

            return BadRequest("عذراً، فشلت عملية تسجيل الموجه. يرجى التأكد من صحة البيانات المرسلة.");
        }

        [HttpPost("register-teacher")]
        public async Task<IActionResult> RegisterNewTeacher([FromBody] CreateTeacherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var generatedAccount = await _deptService.RegisterTeacherWorkflowAsync(dto);

            if (generatedAccount != null)
            {
                // نجاح العملية وإرجاع الـ Account Number مباشرة للواجهة
                return Ok(new
                {
                    message = "تم تسجيل المعلم الجديد في النظام بنجاح.",
                    accountNumber = generatedAccount
                });
            }

            return BadRequest("عذراً، فشلت عملية تسجيل المعلم الجديد. يرجى مراجعة البيانات المدخلة.");
        }

        [HttpPost("create-new-section")]
        public async Task<IActionResult> AutoCreateNextSection([FromBody] CreateAutomaticClassRoomDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _deptService.CreateNextSectionAutomatedAsync(dto);

            return success
                ? Ok(new { message = "تم إنشاء الشعبة التالية المتتابعة لهذا الصف بنجاح في النظام." })
                : BadRequest("عذراً، فشلت عملية الإنشاء التلقائي للشعبة.");
        }

        [HttpGet("supervisor/{id}")]
        public async Task<IActionResult> GetSupervisorById(int id)
        {
            try
            {
                var managerClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(managerClaim, out int managerPersonId))
                    return Unauthorized(new { message = "جلسة المستخدم غير صالحة، يرجى تسجيل الدخول مجدداً." });

                var result = await _deptService.GetSupervisorByIdAsync(managerPersonId, id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء جلب بيانات الموجه.", details = ex.Message });
            }
        }

        [HttpPut("supervisor/{id}")]
        public async Task<IActionResult> UpdateSupervisor(int id, [FromBody] UpdateSupervisorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var managerClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(managerClaim, out int managerPersonId))
                    return Unauthorized(new { message = "جلسة المستخدم غير صالحة، يرجى تسجيل الدخول مجدداً." });

                var result = await _deptService.UpdateSupervisorAsync(managerPersonId, id, dto);
                return Ok(new
                {
                    message = "تم تحديث بيانات الموجه بنجاح.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء تحديث بيانات الموجه.", details = ex.Message });
            }
        }

        [HttpDelete("supervisor/{id}")]
        public async Task<IActionResult> DeleteSupervisor(int id)
        {
            try
            {
                var managerClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(managerClaim, out int managerPersonId))
                    return Unauthorized(new { message = "جلسة المستخدم غير صالحة، يرجى تسجيل الدخول مجدداً." });

                var success = await _deptService.DeleteSupervisorAsync(managerPersonId, id);
                return Ok(new { message = "تم حذف الموجه وسجلاته بنجاح من النظام." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // منع الحذف لوجود ارتباطات ومسؤوليات نشطة (شعب صفية، إشراف معلمين، محادثات)
                return BadRequest(new
                {
                    message = ex.Message,
                    errorCode = "SUPERVISOR_HAS_ACTIVE_DEPENDENCIES"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء معالجة حذف الموجه.", details = ex.Message });
            }
        }

        [HttpPost("assign-supervisor-to-class")]
        public async Task<IActionResult> AssignSupervisorToClass([FromBody] AssignSupervisorToClassDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var managerClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(managerClaim, out int managerPersonId))
                    return Unauthorized(new { message = "جلسة المستخدم غير صالحة، يرجى تسجيل الدخول مجدداً." });

                var result = await _deptService.AssignSupervisorToClassAsync(managerPersonId, dto);
                return Ok(new { message = result.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء إسناد الموجه للشعبة الصفية.", details = ex.Message });
            }
        }

        [HttpDelete("remove-supervisor-from-class/{classRoomId}")]
        public async Task<IActionResult> RemoveSupervisorFromClass(int classRoomId)
        {
            try
            {
                var managerClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(managerClaim, out int managerPersonId))
                    return Unauthorized(new { message = "جلسة المستخدم غير صالحة، يرجى تسجيل الدخول مجدداً." });

                var result = await _deptService.UnassignSupervisorFromClassAsync(managerPersonId, classRoomId);
                return Ok(new { message = result.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء إلغاء إسناد الموجه من الشعبة.", details = ex.Message });
            }
        }

        [HttpPost("classroom-schedule")]
        public async Task<IActionResult> SaveClassRoomSchedule([FromBody] SaveClassRoomScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _deptService.SaveClassRoomScheduleAsync(dto);
            return success
                ? Ok(new { message = "تم حفظ وتحديث جدول دوام الشعبة بنجاح في النظام." })
                : BadRequest("عذراً، فشلت عملية حفظ جدول دوام الشعبة.");
        }

        [HttpPost("teacher-schedule")]
        public async Task<IActionResult> SaveTeacherSchedule([FromBody] SaveTeacherScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _deptService.SaveTeacherScheduleAsync(dto);
            return success
                ? Ok(new { message = "تم حفظ وتحديث جدول دوام الأستاذ بنجاح في النظام." })
                : BadRequest("عذراً، فشلت عملية حفظ جدول دوام الأستاذ.");
        }

    }
}
