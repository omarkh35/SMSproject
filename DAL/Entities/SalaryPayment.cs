using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public partial class SalaryPayment
{
        [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal BaseSalary { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Deduction { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal NetSalary { get; private set; }

    [ForeignKey("EmployeeId")]
    public virtual User Employee { get; set; }

}
