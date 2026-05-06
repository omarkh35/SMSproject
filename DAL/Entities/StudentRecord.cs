using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class StudentRecord
{
    public int StudentRecordId { get; set; }

    public int StudentId { get; set; }

    public string? StudentNumber { get; set; }

    public short StudyYear { get; set; }

    public int Grade { get; set; }

    public decimal YearlyPayment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Student Student { get; set; } = null!;
}
