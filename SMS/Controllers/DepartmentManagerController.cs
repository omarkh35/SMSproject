using BLL.EntitiesDTOS.DepartmentManager;
using BLL.Interfaces;
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

        [HttpPost("classroom")]
        public async Task<IActionResult> CreateClassRoom([FromBody] ClassRoomCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _deptService.CreateClassRoomAsync(dto);
            return Ok(result);
        }

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
        [HttpGet("supervisors-dashboard")]
        public async Task<IActionResult> GetSupervisorsDashboard()
        {
            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _deptService.GetSupervisorsDashboardAsync(managerPersonId);
            return Ok(result);
        }

        [HttpGet("teachers-management")]
        public async Task<IActionResult> GetTeachersManagementDashboard([FromQuery] int page = 1)
        {
            if (page < 1) 
                page = 1;

            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _deptService.GetTeachersManagementDashboardAsync(managerPersonId, page);
            return Ok(result);
        }

        [HttpPost("teachers")]
        public async Task<IActionResult> RegisterNewTeacher([FromBody] CreateTeacherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _deptService.RegisterTeacherWorkflowAsync(dto);

            return success ? Ok(new { message = "Teacher registered successfully with generated account number." })
                           : BadRequest("Failed to register teacher. Phone number or Email might already exist.");
        }

        [HttpGet("students-directory")]
        public async Task<IActionResult> GetStudentDirectory([FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var managerPersonId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _deptService.GetStudentDirectoryDashboardAsync(managerPersonId, page);
            return Ok(result);
        }

    }
}
