using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Accountant
{
    public class AccountantDashboardDto
    {
        public int TotalStudentsCount { get; set; }
        public int TotalPages { get; set; }
        public List<StudentGridItemDto> Students { get; set; } = new();
        public List<ClassDropdownItemDto> AvailableClasses { get; set; } = new();
    }

    public class StudentGridItemDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string ClassAndSection { get; set; } = string.Empty;
        public decimal AnnualFee { get; set; }
    }

    public class ClassDropdownItemDto
    {
        public int ClassRoomID { get; set; }
        public string ClassDisplayName { get; set; } = string.Empty;
    }

    public class StudentRegistrationDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأب مطلوب")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string FamilyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الأم مطلوب")]
        public string MotherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public bool Gender { get; set; }

        public string? StudentPhotoPath { get; set; }
        public string HomeAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم العائلة مطلوب لربط الطالب بولي أمره")]
        public string FamilyNumber { get; set; } = string.Empty;

        [Required] public int GradeID { get; set; }
        [Required] public short AcademicYear { get; set; }
        [Required] public decimal YearlyPayment { get; set; }
    }


    public class StudentDetailsFormDto
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string? StudentPhotoPath { get; set; }
        public string? FamilyNumber { get; set; }
        public string HomeAddress { get; set; } = string.Empty;
        public int GradeID { get; set; }
        public short AcademicYear { get; set; }
    }


    public class ParentAccountsDashboardDto
    {
        public int TotalParentsCount { get; set; }
        public int TotalPages { get; set; }
        public List<ParentGridItemDto> Parents { get; set; } = new();
    }

    public class ParentGridItemDto
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty; 
    }


    public class InstallmentTrackingDashboardDto
    {
        // Core Summary Metrics
        public decimal TotalAmounts { get; set; }
        public decimal PaymentAmounts { get; set; }
        public decimal RemainingToPay { get; set; }

        public int TotalPages { get; set; }
        public List<InstallmentStudentGridItemDto> Students { get; set; } = new();
        public List<ClassDropdownItemDto> AvailableClasses { get; set; } = new();
    }

    public class InstallmentStudentGridItemDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public decimal AnnualFees { get; set; }
        public decimal AmountDue { get; set; }
        public string Status { get; set; } = string.Empty; // "PAID" or "UNPAID"
    }

    public class StudentPaymentDetailsDto
    {
        public decimal TotalFee { get; set; }
        public decimal Balance { get; set; }
        public List<InstallmentHistoryItemDto> InstallmentSchedule { get; set; } = new();
    }

    public class InstallmentHistoryItemDto
    {
        public string PaymentDateStr { get; set; } = string.Empty; // Formatted cleanly as d/M/yyyy
        public decimal AmountPaid { get; set; }
    }

    public class StaffSalaryDashboardDto
    {
        public List<StaffSalaryGridItemDto> StaffMembers { get; set; } = new();
    }

    public class StaffSalaryGridItemDto
    {
        public int PersonID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string WorkHours { get; set; } = string.Empty; // يعرض رقم للمعلم و "-" للبقية
        public string PayPerHour { get; set; } = string.Empty; // يعرض رقم للمعلم و "-" للبقية
        public string Status { get; set; } = string.Empty; // "paid" أو "unpaid"
        public decimal NetSalary { get; set; }
    }

}
