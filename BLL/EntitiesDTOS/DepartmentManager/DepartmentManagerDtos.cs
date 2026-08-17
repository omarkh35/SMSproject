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
        public int? SupervisorId { get; set; }
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


    //public class CreateTeacherDto
    //{
    //    [Required] public string FirstName { get; set; }
    //    [Required] public string SecondName { get; set; }
    //    [Required] public string LastName { get; set; }
    //    [Required] public DateOnly DateOfBirth { get; set; }
    //    [Required] public bool Gender { get; set; } 
    //    [Required] public string PhoneNumber { get; set; }

    //    [EmailAddress] public string? Email { get; set; } 
    //    public string? ClearTextPassword { get; set; }    
    //    public byte? WeeklyClasses { get; set; }          
    //    public decimal? SalaryPerClass { get; set; }      
    //}
    
    public class StudentDirectoryDashboardDto
    {
        public int TotalStudentsCount { get; set; }
        public string PassRate { get; set; } = "N/A"; // نسبة النجاح (Success Rate)
        public int TotalPages { get; set; }
        public List<StudentGridItemDto> Students { get; set; } = new();
    }

    public class StudentGridItemDto
    {
        public int StudentId { get; set; }
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


    public class CreateSupervisorDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        public string SecondName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public bool Gender { get; set; } // true = ذكر، false = أنثى

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        public string? Email { get; set; }



        // 3. معلومات الموجه المالية (Supervisor)
        [Required(ErrorMessage = "قيمة الراتب مطلوبة")]
        [Range(0, 99999999, ErrorMessage = "الراتب يجب أن يكون قيمة منطقية موجبة")]
        public decimal Salary { get; set; }
    }

    public class CreateTeacherDto
    {
        // 1. معلومات الشخص (Person)
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        public string SecondName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public bool Gender { get; set; } // true = ذكر، false = أنثى

        // 2. معلومات حساب المستخدم (User)
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        public string? Email { get; set; }

        // 3. معلومات المعلم التعليمية والمالية (Teacher)
        [Range(0, 100, ErrorMessage = "عدد الحصص الأسبوعية يجب أن يكون بين 0 و 100")]
        public byte? WeeklyClasses { get; set; }

        [Range(0, 99999999, ErrorMessage = "أجرة الحصة يجب أن تكون قيمة منطقية موجبة")]
        public decimal? SalaryPerClass { get; set; }
    }

    public class UpdateSupervisorDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        public string SecondName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        public bool Gender { get; set; } // true = ذكر، false = أنثى

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صالحة")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "قيمة الراتب مطلوبة")]
        [Range(0, 99999999, ErrorMessage = "الراتب يجب أن يكون قيمة موجبة")]
        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class SupervisorDetailsDto
    {
        public int SupervisorId { get; set; }
        public int PersonId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public int AssignedSectionsCount { get; set; }
        public int SupervisedTeachersCount { get; set; }
    }

    public class CreateAutomaticClassRoomDto
    {
        public int GradeID { get; set; }
    }

    public class AssignSupervisorToClassDto
    {
        [Required(ErrorMessage = "معرف الشعبة الصفية مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "معرف الشعبة الصفية يجب أن يكون رقماً موجباً أكبر من الصفر")]
        public int ClassRoomId { get; set; }

        [Required(ErrorMessage = "معرف الموجه مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "معرف الموجه يجب أن يكون رقماً موجباً أكبر من الصفر")]
        public int SupervisorId { get; set; }
    }

    public class AssignSupervisorToClassResultDto
    {
        public int ClassRoomId { get; set; }
        public int GradeId { get; set; }
        public int GradeNumber { get; set; }
        public byte Section { get; set; }
        public short StartYear { get; set; }
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public string SupervisorPhone { get; set; } = string.Empty;
        public int? PreviousSupervisorId { get; set; }
        public string? PreviousSupervisorName { get; set; }
        public bool IsReassignment { get; set; }
        public int TotalStudentsInClass { get; set; }
        public int TotalSupervisedClassesCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UnassignSupervisorFromClassResultDto
    {
        public int ClassRoomId { get; set; }
        public int GradeId { get; set; }
        public int GradeNumber { get; set; }
        public byte Section { get; set; }
        public int RemovedSupervisorId { get; set; }
        public string RemovedSupervisorName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }


}
