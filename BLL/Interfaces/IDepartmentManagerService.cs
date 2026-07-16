using BLL.EntitiesDTOS.DepartmentManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{


    public interface IDepartmentManagerService 
    {
        Task<IEnumerable<ClassRoomDto>> GetAllClassRoomsAsync();
        Task<ClassRoomDto> GetClassRoomByIdAsync(int id); 
        Task<ClassRoomDto> CreateClassRoomAsync(ClassRoomCreateDto dto); 
        Task<bool> UpdateClassRoomAsync(int id, ClassRoomUpdateDto dto);
        Task<bool> DeleteClassRoomAsync(int id);

        Task<bool> AssignStudentToClassAsync(StudentToClassDto dto);
        Task<bool> RemoveStudentFromClassAsync(int studentId, int classRoomId);

        Task<bool> AssignTeacherToClassAsync(TeacherToClassDto dto);
        Task<bool> UpdateTeacherAssignmentAsync(TeacherToClassDto dto);
        Task<bool> RemoveTeacherFromClassAsync(int teacherId, int classRoomId);

        Task<bool> AssignSupervisorToTeacherAsync(TeacherSupervisorDto dto);
        Task<bool> RemoveSupervisorFromTeacherAsync(int supervisorId, int teacherId);

        /////////////////////////////////////////////
        ///
    

        //////////////////////////////////////////
        ///
        Task<StudentDirectoryDashboardDto> GetStudentDirectoryDashboardAsync(int managerPersonId, string? searchName, int page);


        Task<SupervisorsDashboardDto> GetSupervisorsManagementDashboardAsync(int managerPersonId);
        Task<TeachersDashboardDto> GetTeachersManagementDashboardAsync();


    }

}
