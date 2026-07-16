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
        public string PassRate { get; set; } = "N/A"; // نسبة النجاح (Success Rate)
        public int TotalPages { get; set; }
        public List<StudentGridItemDto> Students { get; set; } = new();
    }

    public class StudentGridItemDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty; // STUDENT NAME
        public string Grade { get; set; } = string.Empty;       // GRADE
        public int Section { get; set; }                        // SECTION (رقم نقي تماماً بناءً على طلبك)
        public string Phone { get; set; } = string.Empty;       // PHONE
    }


    public class SupervisorsDashboardDto
    {
        // Top Card Summary Metrics
        public int TotalSupervisors { get; set; }
        public int AssignedSections { get; set; }
        public int OpenSections { get; set; } // Classrooms where SupervisorID is null

        public List<SupervisorGridItemDto> Supervisors { get; set; } = new();
    }

    public class SupervisorGridItemDto
    {
        public int SupervisorID { get; set; }
        public string FullName { get; set; } = string.Empty; // FULL NAME
        public string Phone { get; set; } = string.Empty;      // PHONE
        public string Status { get; set; } = string.Empty;     // STATUS ("Active" or "Inactive")
        public int SectionsCount { get; set; }                  // SECTIONS (Count of rooms supervised)
    }


    public class TeachersDashboardDto
    {
        // البطاقة العلوية
        public int TotalTeachers { get; set; }

        public List<TeacherGridItemDto> Teachers { get; set; } = new();
    }

    public class TeacherGridItemDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; } = string.Empty; // FULL NAME
        public string Phone { get; set; } = string.Empty;     // PHONE
        public int Lessons { get; set; }                      // LESSONS (عدد الحصص الأسبوعية)
    }

}
