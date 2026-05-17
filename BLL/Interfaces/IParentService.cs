using BLL.EntitiesDTOS.Parent;
using BLL.EntitiesDTOS.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IParentService
    {

        Task<IEnumerable<ParentChildDto>> GetMyChildrenAsync(int ParentPersonId);

        Task<ParentDashboardDto> GetParentDashboardAsync(int parentPersonId);

        Task<StudentBagDetailsDto?> GetStudentBagDetailsAsync(int parentPersonId, int studentId);

        Task<StudentScheduleDto?> GetStudentWeeklyScheduleAsync(int parentPersonId, int studentId, string schemeAndHost);


        Task<StudentExamScheduleDto?> GetStudentExamScheduleAsync(int parentPersonId, int studentId, string schemeAndHost);

        Task<StudentAcademicSummaryDto?> GetStudentAcademicSummaryAsync(int parentPersonId, int studentId);

        Task<StudentProfileDto?> GetStudentProfileAsync(int parentPersonId, int studentId, string schemeAndHost);

        Task<StudentAttendanceSummaryDto?> GetStudentAttendanceCalendarAsync(int parentPersonId, int studentId, int year, int month);

        Task<SubjectDetailedReportDto?> GetSubjectDetailedReportAsync(int parentPersonId, int studentId, int subjectId);


    }
}
