using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Grade
{
    public int GradeId { get; set; }

    public int GradeNumber { get; set; }

    public virtual ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();

    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();

    public virtual ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();

    public virtual ICollection<StudentRecord> StudentRecords { get; set; } = new List<StudentRecord>();
}
