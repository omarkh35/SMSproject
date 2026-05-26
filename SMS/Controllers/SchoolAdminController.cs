using BLL.EntitiesDTOS.SchoolAdmin;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolAdminController : ControllerBase
    {
        private readonly ISchoolAdminService _adminService;
        public SchoolAdminController(ISchoolAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _adminService.GetAllSubjectsAsync();
            return Ok(subjects);
        }
        [HttpPost("subject")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminService.CreateSubjectAsync(dto);
            return Ok(result);
        }
        [HttpPut("subject/{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] SubjectUpdateDto dto)
        {
            var result = await _adminService.UpdateSubjectAsync(id, dto);
            if (!result) return NotFound("المادة غير موجودة");

            return Ok(new { message = "تم التعديل بنجاح" });
        }

        [HttpDelete("subject/{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var result = await _adminService.DeleteSubjectAsync(id);
            if (!result) return NotFound("المادة غير موجودة");

            return Ok(new { message = "تم الحذف بنجاح" });
        }


        [HttpGet("department-managers")]
        public async Task<IActionResult> GetAllDepartmentManagers()
        {
            var managers = await _adminService.GetAllDepartmentManagersAsync();
            return Ok(managers);
        }

        [HttpPost("department-manager")]
        public async Task<IActionResult> AddDepartmentManager([FromBody] DepartmentManagerCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminService.AddDepartmentManagerAsync(dto);
            return Ok(result);
        }

        [HttpPut("department-manager/{id}")]
        public async Task<IActionResult> UpdateDepartmentManager(int id, [FromBody] StaffUpdateDto dto)
        {
            var success = await _adminService.UpdateDepartmentManagerAsync(id, dto);
            if (!success) return NotFound("مدير القسم غير موجود");

            return Ok(new { message = "تم تحديث بيانات مدير القسم بنجاح" });
        }

        [HttpDelete("department-manager/{id}")]
        public async Task<IActionResult> DeleteDepartmentManager(int id)
        {
            var success = await _adminService.DeleteDepartmentManagerAsync(id);
            if (!success) return NotFound("مدير القسم غير موجود");

            return Ok(new { message = "تم حذف مدير القسم بنجاح" });
        }


        [HttpGet("supervisors")]
        public async Task<IActionResult> GetAllSupervisors()
        {
            var supervisors = await _adminService.GetAllSupervisorsAsync();
            return Ok(supervisors);
        }

        [HttpPost("supervisor")]
        public async Task<IActionResult> AddSupervisor([FromBody] SupervisorCreateDto dto)
        {
            var result = await _adminService.AddSupervisorAsync(dto);
            return Ok(result);
        }

        [HttpPut("supervisor/{id}")]
        public async Task<IActionResult> UpdateSupervisor(int id, [FromBody] StaffUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _adminService.UpdateSupervisorAsync(id, dto);
            if (!success) return NotFound("الموجه غير موجود");

            return Ok(new { message = "تم تحديث بيانات الموجه بنجاح" });
        }

        [HttpDelete("supervisor/{id}")]
        public async Task<IActionResult> DeleteSupervisor(int id)
        {
            var success = await _adminService.DeleteSupervisorAsync(id);
            if (!success) return NotFound("الموجه غير موجود");

            return Ok(new { message = "تم حذف الموجه بنجاح" });
        }

    }
}
