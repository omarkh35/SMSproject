using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ExamType
{
    public int ExamTypeId { get; set; }

    public string ExamTypeName { get; set; } = null!;

    public byte Semester { get; set; }

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();
}
