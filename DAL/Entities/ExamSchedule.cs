using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ExamSchedule
{
    public int ExamScheduleId { get; set; }

    public int GradeId { get; set; }

    public byte Semester { get; set; }

    public string ImagePath { get; set; } = null!;

    public short AcademicYear { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Grade Grade { get; set; } = null!;
}
