using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.DepartmentManager
{
    public class ClassRoomDto
    {
        public int Id { get; set; }
        public int GradeId { get; set; }
        public byte Section { get; set; } 
        public int? SupervisorId { get; set; }
        public short StartYear { get; set; }
        public int CurrentStudentsCount { get; set; }
    }

    public class ClassRoomCreateDto     
    {
        public int GradeId { get; set; }
        public byte Section { get; set; }
        public short StartYear { get; set; }
        public int? SupervisorId { get; set; }
    }

    public class ClassRoomUpdateDto 
    {
        public byte? Section { get; set; }
        public short? StartYear { get; set; }
    }

    public class StudentToClassDto
    {
        public int StudentId { get; set; }
        public int ClassRoomId { get; set; }
    }

    public class TeacherToClassDto
    {
        public int ClassroomTeacherId { get; set; }
        public int TeacherId { get; set; }
        public int ClassRoomId { get; set; }
        public int SubjectId { get; set; }
    }

    public class TeacherSupervisorDto
    {
        public int SupervisorId { get; set; }
        public int TeacherId { get; set; }
    }

    public class SupervisorsDashboardDto
    {
        public int TotalActiveSupervisors { get; set; }
        public int AssignedSectionsCount { get; set; }
        public int UnassignedClassesCount { get; set; }
        public List<SupervisorGridItemDto> Supervisors { get; set; } = new();
        public int TotalCount { get; set; } // For pagination calculation
    }

    public class SupervisorGridItemDto
    {
        public int SupervisorID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ProfessionalTitle { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> AssignedClasses { get; set; } = new();
    }

    public class TeachersDashboardDto
    {
        public int TotalTeachersCount { get; set; }
        public string AvgWorkingHours { get; set; } = string.Empty;
        public int TotalPages { get; set; }
        public List<TeacherGridItemDto> Teachers { get; set; } = new();
    }

    public class TeacherGridItemDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = string.Empty;
    }

    public class CreateTeacherDto
    {
        [Required] public string FirstName { get; set; }
        [Required] public string SecondName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required] public bool Gender { get; set; } 
        [Required] public string PhoneNumber { get; set; }

        [EmailAddress] public string? Email { get; set; } 
        public string? ClearTextPassword { get; set; }    
        public byte? WeeklyClasses { get; set; }          
        public decimal? SalaryPerClass { get; set; }      
    }
    public class StudentDirectoryDashboardDto
    {
        public int TotalStudentsCount { get; set; }
        public string PassRate { get; set; } 
        public List<StudentGridItemDto> Students { get; set; } = new();
        public int TotalPages { get; set; }
    }

    public class StudentGridItemDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; } 
        public string ClassAndSection { get; set; } 
        public string ParentPhoneNumber { get; set; }
    }


}
