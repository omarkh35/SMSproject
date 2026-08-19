using BLL.EntitiesDTOS.SchoolAdmin;
using BLL.EntitiesDTOS.General;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class SchoolAdminController : ControllerBase
    {
        private readonly ISchoolAdminService _adminService;
        private readonly ISchoolSettingService _schoolSettingService;

        public SchoolAdminController(
            ISchoolAdminService adminService,
            ISchoolSettingService schoolSettingService)
        {
            _adminService = adminService;
            _schoolSettingService = schoolSettingService;

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
            if (result == null) return BadRequest(new { message = "فشلت عملية إنشاء المادة التعليمية." });

            return Ok(result);
        }

        [HttpPut("subject/{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] SubjectUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminService.UpdateSubjectAsync(id, dto);
            if (!result) return NotFound("المادة غير موجودة");

            return Ok(new { message = "تم التعديل بنجاح" });
        }

        [HttpDelete("subject/{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            try
            {
                var result = await _adminService.DeleteSubjectAsync(id);
                if (!result)
                    return NotFound(new { message = "المادة التعليمية المطلوبة غير موجودة في النظام." });

                return Ok(new { message = "تم حذف المادة بنجاح" });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                {
                    return BadRequest(new
                    {
                        error = "لا يمكن حذف هذه المادة حالياً لوجود علامات ودرجات أكاديمية مسجلة بها للطلاب. يمكنك تجميدها بدلاً من الحذف."
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "حدث خطأ أثناء تحديث البيانات في السيرفر." });
            }
        }

        [HttpGet("department-managers-detail")]
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
            if (result == null) return BadRequest(new { message = "فشلت عملية إضافة مدير القسم، يرجى مراجعة البيانات." });

            return Ok(result); // يعيد كائن المدير كاملاً مع الـ AccountNumber المتولد
        }

        [HttpPut("department-manager/{id}")]
        public async Task<IActionResult> UpdateDepartmentManager(int id, [FromBody] StaffUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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

       
        [HttpGet("supervisors-detail")]
        public async Task<IActionResult> GetAllSupervisors()
        {
            var supervisors = await _adminService.GetAllSupervisorsAsync();
            return Ok(supervisors);
        }

        [HttpPost("supervisor")]
        public async Task<IActionResult> AddSupervisor([FromBody] SupervisorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _adminService.AddSupervisorAsync(dto);
            if (result == null) return BadRequest(new { message = "فشلت عملية إضافة الموجه، يرجى مراجعة البيانات." });

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

        [HttpGet("main-dashboard")]
        public async Task<IActionResult> GetAdminDashboardMetrics()
        {
            var result = await _adminService.GetMainDashboardMetricsAsync();
            return Ok(result);
        }


        [HttpGet("teachers-general")]
        public async Task<IActionResult> GetTeachersDirectoryGrid([FromQuery] string? searchName, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _adminService.GetTeachersManagementGridAsync(searchName, page);
            return Ok(result);
        }

        [HttpGet("supervisors-general")]
        public async Task<IActionResult> GetSupervisorsDirectoryGrid([FromQuery] string? searchName, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _adminService.GetSupervisorsManagementGridAsync(searchName, page);
            return Ok(result);
        }

        [HttpGet("departmentmanagers-general")]
        public async Task<IActionResult> GetDepartmentManagersDirectoryGrid([FromQuery] string? searchName, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _adminService.GetDepartmentManagersGridAsync(searchName, page);
            return Ok(result);
        }


        [HttpGet("students-directory-grid")]
        public async Task<IActionResult> GetAdminStudentsDirectory(
    [FromQuery] string? searchName,
    [FromQuery] int? gradeId,
    [FromQuery] int? sectionNumber,
    [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _adminService.GetStudentsManagementGridAsync(searchName, gradeId, sectionNumber, page);
            return Ok(result);
        }


        [HttpGet("grade-configuration/{gradeId}")]
        public async Task<IActionResult> GetGradeConfiguration(int gradeId)
        {
            var result = await _adminService.GetGradeConfigurationAsync(gradeId);
            return Ok(result);
        }

        [HttpPost("save-grade-subjects")]
        public async Task<IActionResult> SaveGradeSubjects([FromBody] SaveGradeSubjectsDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var success = await _adminService.SaveGradeSubjectsConfigurationAsync(dto);
            return success
                ? Ok(new { message = "تم تحديث  المواد المسندة لهذا الصف الدراسي بنجاح." })
                : BadRequest("عذراً، فشلت العملية.");
        }

        [HttpPost("save-exam-schedule")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveExamSchedule([FromForm] SaveExamScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _adminService.SaveExamScheduleAsync(dto);
            return success
                ? Ok(new { message = "تم حفظ برنامج الامتحان لهذا الصف بنجاح." })
                : BadRequest("عذراً، فشلت عملية حفظ برنامج الامتحان.");
        }

        [HttpPost("announcement")]
        public async Task<IActionResult> CreateSchoolAnnouncement([FromBody] SchoolAnnouncementCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int personId = 0;
            var personIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(personIdClaim, out int parsedId))
            {
                personId = parsedId;
            }

            var result = await _adminService.CreateSchoolAnnouncementAsync(dto, personId);
            return Ok(new
            {
                message = "تم نشر الإعلان المدرسي بنجاح.",
                data = result
            });
        }

      
        [HttpGet("finance")]
        public async Task<IActionResult> GetFinanceDashboard()
        {
            var result = await _adminService.GetFinanceDashboardAsync();
            return Ok(result);
        }

        
        [HttpPut("finance/tuition-fee")]
        public async Task<IActionResult> UpdateTuitionFee([FromBody] UpdateGradeTuitionFeeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _adminService.UpdateGradeTuitionFeeAsync(dto);
            return success
                ? Ok(new { message = "تم تعديل القسط الدراسي للصف وتحديث سجلات الطلاب بنجاح." })
                : BadRequest(new { message = "فشلت عملية تعديل القسط الدراسي، يرجى التحقق من معرف الصف." });
        }

        
        [HttpGet("school-info")]
        public async Task<IActionResult> GetSchoolInfo()
        {
            var info = await _schoolSettingService.GetSchoolInfoAsync();
            return Ok(info);
        }

        [HttpPut("school-info")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateSchoolInfo([FromForm] UpdateSchoolInfoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _schoolSettingService.UpdateSchoolInfoAsync(dto);

                // التقاط الـ Host والـ Scheme الحركي لجهازك المحلي تلقائياً دون كتابته يدوياً
                string schemeAndHost = $"{Request.Scheme}://{Request.Host}";
                string fullLogoUrl = string.Empty;

                // فحص أمان قاطع: إذا كان اللوغو موجوداً نبني الرابط الكامل، وإذا كان NULL نضع رابطاً افتراضياً أو نتركه فارغاً بسلام
                if (!string.IsNullOrEmpty(result.SchoolLogo))
                {
                    string cleanLogoPath = result.SchoolLogo.Replace("\\", "/").TrimStart('/');
                    fullLogoUrl = $"{schemeAndHost}/{cleanLogoPath}";
                }
                else
                {
                    // يمكنك وضع مسار لصورة افتراضية في الـ wwwroot أو تركها فارغة تماماً بناءً على رغبتك
                    fullLogoUrl = $"{schemeAndHost}/uploads/logos/default_logo.png";
                }

                // إسناد الرابط الآمن النهائي للنتيجة المرجوعة للواجهة
                result.SchoolLogo = fullLogoUrl;

                return Ok(new
                {
                    message = "تم حفظ وتحديث بيانات وشعار المدرسة بنجاح واستهداف ممرات التخزين المحلية.",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ غير متوقع أثناء تحديث بيانات المدرسة.",
                    details = ex.Message
                });
            }
        }



    }
}
