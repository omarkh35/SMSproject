using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class SalaryPayment
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal Deduction { get; set; }

    public decimal? NetSalary { get; set; }

    public virtual User Employee { get; set; } = null!;
}
