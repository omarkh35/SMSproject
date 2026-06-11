using BLL.EntitiesDTOS.Supervisor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ISupervisorService
    {
        Task<SupervisorMainDashboardDto> GetMainDashboardAsync(int supervisorPersonId);

        Task<ClassRollCallDto?> GetClassroomRollCallAsync(int supervisorPersonId, int classRoomId);
        Task<bool> DeleteTaskAsync(int supervisorPersonId, long taskId);
        Task<bool> ToggleTaskAsync(int supervisorPersonId, long taskId);
        Task<bool> CreateTaskAsync(int supervisorPersonId, CreateTaskDto dto);
        Task<IEnumerable<SupervisorTaskDto>> GetTodayTasksAsync(int supervisorPersonId);
        Task<AttendanceSheetLoadDto?> LoadAttendanceSheetAsync(int supervisorPersonId, int classRoomId);
        Task<bool> SaveAttendanceSheetWorkflowAsync(int supervisorPersonId, SaveAttendanceSheetDto dto);
        Task<AnnouncementManagementPageDto> LoadAnnouncementsPanelAsync(int senderPersonId);
        Task<bool> PublishAnnouncementAsync(int senderPersonId, CreateAnnouncementRequestDto dto);
        Task<bool> DeleteAnnouncementAsync(int senderPersonId, int announcementId);
        Task<IEnumerable<SupervisorClassCardDto>> GetSupervisedClassroomsDirectoryAsync(int supervisorPersonId);
        Task<IEnumerable<SupervisorStudentGridDto>> GetMyStudentsDirectoryAsync(int supervisorPersonId, int? classRoomId, string searchTerm);
        Task<StudentDetailsPageDto?> GetStudentDetailedProfileAsync(int studentId, int month, int year);
        Task<IEnumerable<SupervisorTeacherSidebarDto>> GetSupervisedTeachersSidebarAsync(int supervisorPersonId, string searchTerm);
        Task<TeacherDetailsPaneDto?> GetTeacherPaneDetailsAsync(int teacherId, int month, int year);

    }
}
