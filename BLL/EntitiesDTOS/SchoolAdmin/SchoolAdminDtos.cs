using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.SchoolAdmin
{


    public class SubjectDto
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = null!;
    }

    public class SubjectCreateDto
    {
        public string SubjectName { get; set; } = null!;
    }

    public class SubjectUpdateDto
    {
        public string SubjectName { get; set; } = null!;
    }

    public class StaffDto
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string FullName { get; set; } = null!;
        public decimal? Salary { get; set; }
        public string Role { get; set; } = null!;
        public string AccountNumber { get; set; } = string.Empty; // حقل رقم الحساب المولد المضاف حديثاً
        public string? DepartmentManagerName { get; set; }
    }

    public class DepartmentManagerCreateDto
    {
        // تم التوسيع لاستقبال السجل الشخصي الكامل للموظف لإنشائه من الصفر
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public decimal Salary { get; set; }
    }

    public class SupervisorCreateDto
    {
        // تم التوسيع لاستقبال السجل الشخصي الكامل للموجه لإنشائه من الصفر
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentManagerId { get; set; }
    }

    public class StaffUpdateDto
    {
        public decimal? Salary { get; set; }
    }

    public class GradeSubjectDto
    {
        public int GradeId { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public string? GradeName { get; set; }
    }

    public class AdminDashboardDto
    {
        // البطاقات العلوية (Top Cards)
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalDepartmentManagers { get; set; }
        public int TotalSupervisors { get; set; }
        public string SuccessRate { get; set; } = "0.0%";

        // القسم الجديد المضاف: شريط الإعلانات (Announcements Carousel Feed)
        public List<DashboardAnnouncementItemDto> Announcements { get; set; } = new();

        // الجدول السفلي الأول: الطلاب لكل صف
        public List<StudentsPerGradeGridItemDto> StudentsPerGrade { get; set; } = new();
        public int TTotalStudents { get; set; }
        public int TotalSections { get; set; }

        // الجدول السفلي الثاني: المعلمون لكل صف
        public List<TeachersPerGradeGridItemDto> TeachersPerGrade { get; set; } = new();
        public int TTotalTeachers { get; set; }
        public int TotalSubjects { get; set; }
    }

    public class StudentsPerGradeGridItemDto
    {
        public string GradeName { get; set; } = string.Empty; // مثل "Grade 9"
        public int StudentsCount { get; set; }
        public int SectionsCount { get; set; }
    }

    public class TeachersPerGradeGridItemDto
    {
        public string GradeName { get; set; } = string.Empty;
        public int TeachersCount { get; set; }
        public int SubjectsCount { get; set; }
    }

    public class DashboardAnnouncementItemDto
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string BodySummary { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty; // يعرض "All" أو "Parents" بناءً على منطق جدولك
        public string CreatedDateStr { get; set; } = string.Empty; // صيغة التاريخ المنسق مثل "Jul 15, 2026"
    }


    public class AdminTeachersDashboardDto
    {
        public int TotalTeachersCount { get; set; }
        public int TotalPages { get; set; }
        public List<AdminTeacherGridItemDto> Teachers { get; set; } = new();
    }

    public class AdminTeacherGridItemDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; } = string.Empty;     // FULL NAME
        public string Status { get; set; } = string.Empty;       // STATUS (Active, Inactive)
        public string Grades { get; set; } = string.Empty;       // GRADES (e.g., "9, 10")
        public decimal Salary { get; set; }                      // SALARY
        public string Phone { get; set; } = string.Empty;         // PHONE
    }

    public class AdminSupervisorsDashboardDto
    {
        public int TotalSupervisorsCount { get; set; }
        public int TotalPages { get; set; }
        public List<AdminSupervisorGridItemDto> Supervisors { get; set; } = new();
    }

    public class AdminSupervisorGridItemDto
    {
        public int SupervisorID { get; set; }
        public string FullName { get; set; } = string.Empty;     // FULL NAME
        public string Phone { get; set; } = string.Empty;        // PHONE
        public string Status { get; set; } = string.Empty;       // STATUS
        public string Sections { get; set; } = string.Empty;     // SECTIONS (التنسيق المخصص: 5(2,3),4(7))
        public decimal Salary { get; set; }                      // SALARY
    }

    public class AdminManagersDashboardDto
    {
        public int TotalManagersCount { get; set; }
        public int TotalPages { get; set; }
        public List<AdminManagerGridItemDto> Managers { get; set; } = new();
    }

    public class AdminManagerGridItemDto
    {
        public int DepartmentManagerID { get; set; }
        public string FullName { get; set; } = string.Empty;  // FULL NAME
        public string Status { get; set; } = string.Empty;    // STATUS
        public string Phone { get; set; } = string.Empty;     // PHONE
        public decimal Salary { get; set; }                   // SALARY
    }


    public class AdminStudentsDashboardDto
    {
        public int TotalStudentsCount { get; set; }
        public int TotalPages { get; set; }
        public List<AdminStudentGridItemDto> Students { get; set; } = new();
        public List<GradeDropdownItemDto> AvailableGrades { get; set; } = new();
        public List<int> AvailableSections { get; set; } = new();
    }

    public class AdminStudentGridItemDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty; // STUDENT NAME
        public string Grade { get; set; } = string.Empty;       // GRADE (e.g., "Grade 9")
        public int Section { get; set; }                        // SECTION (رقم نقي تماماً)
    }

    public class GradeDropdownItemDto
    {
        public int GradeID { get; set; }
        public string GradeDisplayName { get; set; } = string.Empty; // مثل "Grade 9"
    }


    public class GradeConfigViewDto
    {
        public int GradeID { get; set; }
        public List<SubjectConfigItemDto> AllSubjects { get; set; } = new();
    }

    public class SubjectConfigItemDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; } // true إذا كانت المادة مربوطة بهذا الصف مسبقاً (Checked)
    }

    public class SaveGradeSubjectsDto
    {
        [Required] public int GradeID { get; set; }
        public List<int> SelectedSubjectIDs { get; set; } = new(); // قائمة بجميع المعرفات التي تم اختيارها في الواجهة
    }

    // كائن حفظ جدول الامتحانات (Exam Schedule Request)
    public class SaveExamScheduleDto
    {
        [Required(ErrorMessage = "معرف الصف مطلوب")]
        public int GradeID { get; set; }

        [Required(ErrorMessage = "تحديد الفصل الدراسي مطلوب (1 أو 2)")]
        [Range(1, 2, ErrorMessage = "الفصل الدراسي يجب أن يكون 1 أو 2")]
        public byte Semester { get; set; }

        [Required(ErrorMessage = "مسار صورة الجدول مطلوب")]
        //public string? ImagePath { get; set; }

        // ملف صورة جدول الامتحانات الفعلي
        public IFormFile? ScheduleImageFile { get; set; }

        [Required(ErrorMessage = "السنة الدراسية مطلوبة")]
        public short AcademicYear { get; set; } // e.g., 2026
    }


    public class SchoolAnnouncementCreateDto
    {
        [Required(ErrorMessage = "عنوان الإعلان مطلوب")]
        [MaxLength(100, ErrorMessage = "يجب ألا يتجاوز العنوان 100 حرف")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "نص الإعلان مطلوب")]
        public string Content { get; set; } = null!;

        public bool IsGeneral { get; set; } = true; // الافتراضي هو إعلان عام لكل المدرسة
    }

    public class SchoolAnnouncementResponseDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsGeneral { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }

    // =========================================================================
    // كائنات واجهة المالية والرواتب والأقساط لمدير المدرسة (School Finance DTOs)
    // =========================================================================
    public class AdminFinanceDashboardDto
    {
        // 1. البطاقات العلوية الإجمالية (Top Cards)
        public decimal TotalPayments { get; set; }            // $35,000 (Salaries paid to staff)
        public decimal TotalReceivables { get; set; }         // $325,000 (Tuition fees from 200 students)
        public decimal NetBalance { get; set; }                // $290,000 (Remaining after salaries)

        // 2. إحصائيات التذييل الإجمالية (Footer Totals)
        public int TotalStudentsCount { get; set; }           // 200 students
        public decimal TotalTuitionReceivables { get; set; }   // $325,000

        // 3. جدول الأقساط الدراسية حسب الصفوف (Tuition Fees by Grade Grid)
        public List<GradeTuitionFeeGridItemDto> TuitionFeesByGrade { get; set; } = new();
    }

    public class GradeTuitionFeeGridItemDto
    {
        public int GradeId { get; set; }
        public int GradeNumber { get; set; }                  // 9, 10, 11, 12
        public string GradeName { get; set; } = string.Empty; // "Grade 9"
        public decimal TuitionFee { get; set; }               // $1,200 (Class tuition fee)
        public int StudentsCount { get; set; }                // 50 (Number of students registered)
        public decimal TotalAmount { get; set; }              // $60,000 (TuitionFee * StudentsCount)
    }

    public class UpdateGradeTuitionFeeDto
    {
        [Required(ErrorMessage = "معرف الصف مطلوب")]
        public int GradeId { get; set; }

        [Required(ErrorMessage = "قيمة القسط الدراسي مطلوبة")]
        [Range(0, 100000000, ErrorMessage = "يجب أن تكون الرسوم الدراسية قيمة موجبة")]
        public decimal TuitionFee { get; set; }
    }


    public class CreateAccountantDto
    {
        [Required(ErrorMessage = "الاسم الأول للمحاسب مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول لا يجب أن يتجاوز 50 حرف")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        [StringLength(50, ErrorMessage = "اسم الأب لا يجب أن يتجاوز 50 حرف")]
        public string SecondName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [StringLength(50, ErrorMessage = "اسم العائلة لا يجب أن يتجاوز 50 حرف")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public bool Gender { get; set; } // true = ذكر، false = أنثى

        [Required(ErrorMessage = "رقم الهاتف مطلوب للاتصال")]
        [Phone(ErrorMessage = "رقم الهاتف المدخل غير صالح")]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "قيمة الراتب الشهري مطلوبة")]
        [Range(0, 9999999, ErrorMessage = "الراتب يجب أن يكون قيمة موجبة منطقية")]
        public decimal Salary { get; set; }
    }

    public class AdminAccountantsDashboardDto
    {
        public int TotalAccountantsCount { get; set; }
        public int TotalPages { get; set; }
        public List<AdminAccountantGridItemDto> Accountants { get; set; } = new();
    }

    public class AdminAccountantGridItemDto
    {
        public int AccountantID { get; set; }
        public string FullName { get; set; } = string.Empty;     // اسم المحاسب الأول والأخير مدمجين
        public string Phone { get; set; } = string.Empty;        // رقم الهاتف
        public decimal Salary { get; set; }                      // الراتب
        public string AccountNumber { get; set; } = string.Empty; // الـ Account number
    }


    public class UpdateTeacherDto
    {
        [Required(ErrorMessage = "الاسم الأول للأستاذ مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول لا يجب أن يتجاوز 50 حرف")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        [StringLength(50, ErrorMessage = "اسم الأب لا يجب أن يتجاوز 50 حرف")]
        public string SecondName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [StringLength(50, ErrorMessage = "اسم العائلة لا يجب أن يتجاوز 50 حرف")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public bool Gender { get; set; } // true = ذكر، false = أنثى

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف المدخل غير صالح")]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صالحة")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Range(0, 100, ErrorMessage = "عدد الحصص الأسبوعية يجب أن يكون بين 0 و 100")]
        public byte? WeeklyClasses { get; set; }

        [Range(0, 999999, ErrorMessage = "أجرة الحصة يجب أن تكون قيمة منطقية موجبة")]
        public decimal? SalaryPerClass { get; set; }
    }


}