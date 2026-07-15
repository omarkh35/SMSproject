using BLL.EntitiesDTOS.Accountant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAccountantService
    {

        Task<AccountantDashboardDto> GetMainDashboardGridAsync(string? searchName, int? classRoomId, int page);

        Task<bool> RegisterNewStudentAsync(StudentRegistrationDto dto);
        Task<StudentDetailsFormDto?> GetStudentDetailsForFormAsync(int studentId);
        Task<bool> UpdateStudentRegistrationAsync(int studentId, StudentRegistrationDto dto);
        Task<bool> DeleteStudentRecordWorkflowAsync(int studentId);

    }
}
