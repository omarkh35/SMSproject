using BLL.EntitiesDTOS.Parent;
using BLL.EntitiesDTOS.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITeacherService
    {


        Task<TeacherDashboardDto?> GetTeacherDashboardAsync(int teacherPersonId);


        Task<TeacherDetailedProfileDto?> GetTeacherDetailedProfileAsync(int teacherPersonId);

        Task<bool> SaveStudentGradesAsync(SaveGradesBulkDto inputDto);
        Task<IEnumerable<TeacherClassChipDto>> GetTeacherClassesChipsAsync(int teacherPersonId);
        Task<IEnumerable<ClassStudentListDto>> GetStudentsInClassAsync(int classRoomId);

        Task<bool> SaveBulkAttendanceAsync(SaveAttendanceBulkDto inputDto);

        Task<IEnumerable<ClassStudentListDto>> GetStudentsInClassWithSearchAsync(int classRoomId, string? searchName);
        Task<bool> SaveStudentNoteAsync(int teacherPersonId, SaveStudentNoteDto noteDto);

        Task<bool> SaveDailyLessonAsync(int teacherPersonId, SaveDailyLessonDto dto);

        Task<bool> CreateHomeworkAssignmentAsync(int teacherPersonId, SaveHomeworkDto dto);

    }
}
