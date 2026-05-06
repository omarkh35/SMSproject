using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class Supervisor
{
    public int SupervisorId { get; set; }

    public int DepartmentManagerId { get; set; }

    public int UserId { get; set; }

    public decimal? Salary { get; set; }

    public virtual ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();

    public virtual DepartmentManager DepartmentManager { get; set; } = null!;

    public virtual ICollection<TeacherSupervisor> TeacherSupervisors { get; set; } = new List<TeacherSupervisor>();

    public virtual User User { get; set; } = null!;
}
