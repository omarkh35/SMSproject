using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public int PersonId { get; set; }

    public byte? WeeklyClasses { get; set; }

    public decimal? SalaryPerClass { get; set; }

    public virtual ICollection<ClassroomTeacher> ClassroomTeachers { get; set; } = new List<ClassroomTeacher>();

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<TeacherAttendance> TeacherAttendances { get; set; } = new List<TeacherAttendance>();

    public virtual ICollection<TeacherSupervisor> TeacherSupervisors { get; set; } = new List<TeacherSupervisor>();
}
