using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class GradeSubject
{
    public int GradeSubjectId { get; set; }

    public int GradeId { get; set; }

    public int SubjectId { get; set; }

    public virtual Grade Grade { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
