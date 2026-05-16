using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class StudentRecord
{
    public int StudentRecordId { get; set; }

    public int StudentId { get; set; }

    public short StudyYear { get; set; }

    public int GradeId { get; set; }

    public decimal YearlyPayment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Grade Grade { get; set; } = null!;

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Student Student { get; set; } = null!;
}
