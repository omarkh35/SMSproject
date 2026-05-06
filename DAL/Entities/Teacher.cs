using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public int UserId { get; set; }

    public byte? WeeklyClasses { get; set; }

    public decimal? SalaryPerClass { get; set; }

    public virtual ICollection<ClassroomTeacher> ClassroomTeachers { get; set; } = new List<ClassroomTeacher>();

    public virtual ICollection<TeacherSupervisor> TeacherSupervisors { get; set; } = new List<TeacherSupervisor>();

    public virtual User User { get; set; } = null!;
}
