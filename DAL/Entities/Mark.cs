using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Mark
{
    public int MarkId { get; set; }

    public int StudentRecordId { get; set; }

    public int SubjectId { get; set; }

    public int ExamTypeId { get; set; }

    public decimal MarkValue { get; set; }

    public decimal FullMark { get; set; }

    public DateOnly ExamDate { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// 0 = Pending Review, 1 = Live to Parents
    /// </summary>
    public bool IsApproved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ExamType ExamType { get; set; } = null!;

    public virtual StudentRecord StudentRecord { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
