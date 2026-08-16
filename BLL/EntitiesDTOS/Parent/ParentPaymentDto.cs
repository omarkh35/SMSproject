using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class MakeStudentPaymentRequestDto
    {
        [Required(ErrorMessage = "معرف الطالب مطلوب.")]
        public int StudentID { get; set; }

        [Required(ErrorMessage = "المبلغ المراد دفعه مطلوب.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من الصفر.")]
        public decimal Amount { get; set; }
    }

    public class StudentPaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? PaymentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PreviousWalletBalance { get; set; }
        public decimal NewWalletBalance { get; set; }
        public decimal TotalAnnualFee { get; set; }
        public decimal TotalPaidSoFar { get; set; }
        public decimal RemainingFeeDue { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class ParentWalletDto
    {
        public int ParentId { get; set; }
        public decimal WalletBalance { get; set; }
        public string? FamilyCardNumber { get; set; }
    }

    public class ParentStudentPaymentSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal TotalAnnualFee { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingDue { get; set; }
        public bool IsFullyPaid { get; set; }
        public decimal ParentWalletBalance { get; set; }
    }

}
