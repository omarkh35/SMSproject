using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public virtual ICollection<ClassroomTeacher> ClassroomTeachers { get; set; } = new List<ClassroomTeacher>();

    public virtual ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();
}
