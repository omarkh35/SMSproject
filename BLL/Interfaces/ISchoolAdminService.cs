using BLL.EntitiesDTOS.SchoolAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{

    public interface ISchoolAdminService
    {
    
        Task<IEnumerable<StaffDto>> GetAllDepartmentManagersAsync();
        Task<StaffDto> AddDepartmentManagerAsync(DepartmentManagerCreateDto dto); 
        Task<bool> UpdateDepartmentManagerAsync(int id, StaffUpdateDto dto);
        Task<bool> DeleteDepartmentManagerAsync(int id);

        Task<IEnumerable<StaffDto>> GetAllSupervisorsAsync();
        Task<StaffDto> AddSupervisorAsync(SupervisorCreateDto dto); 
        Task<bool> UpdateSupervisorAsync(int id, StaffUpdateDto dto);
        Task<bool> DeleteSupervisorAsync(int id);

        Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync();
        Task<SubjectDto> CreateSubjectAsync(SubjectCreateDto dto);
        Task<bool> UpdateSubjectAsync(int id, SubjectUpdateDto dto);
        Task<bool> DeleteSubjectAsync(int id);

        Task<GradeSubjectDto> AssignSubjectToGradeAsync(GradeSubjectDto dto);
        Task<bool> RemoveSubjectFromGradeAsync(int gradeId, int subjectId);
        Task<AdminDashboardDto> GetMainDashboardMetricsAsync();

        Task<AdminTeachersDashboardDto> GetTeachersManagementGridAsync(string? searchName, int page);

        Task<AdminSupervisorsDashboardDto> GetSupervisorsManagementGridAsync(string? searchName, int page);

        Task<AdminManagersDashboardDto> GetDepartmentManagersGridAsync(string? searchName, int page);

        Task<AdminStudentsDashboardDto> GetStudentsManagementGridAsync(string? searchName, int? gradeId, int? sectionNumber, int page);

        Task<GradeConfigViewDto> GetGradeConfigurationAsync(int gradeId);
        Task<bool> SaveGradeSubjectsConfigurationAsync(SaveGradeSubjectsDto dto);

        // 2. الـ API الثاني لحفظ جدول الامتحانات للصف
        Task<bool> SaveExamScheduleAsync(SaveExamScheduleDto dto);


    }

}
