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
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string FatherName { get; set; } = string.Empty;
        [Required] public string FamilyName { get; set; } = string.Empty;
        [Required] public string MotherName { get; set; } = string.Empty;
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required] public bool Gender { get; set; } // true = Male, false = Female (or vice versa based on your schema mapping)

        public string? StudentPhotoPath { get; set; }
        public string? FamilyNumber { get; set; } // FamilyCardNumber
        [Required] public string HomeAddress { get; set; } = string.Empty;

        [Required] public int GradeID { get; set; } // The Class dropdown select value
        [Required] public short AcademicYear { get; set; } // e.g., 2026
        [Required] public decimal YearlyPayment { get; set; } // Populated on back-end or hidden field
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
}
