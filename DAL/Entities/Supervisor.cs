using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public partial class Supervisor
{
    public int SupervisorId { get; set; }

    public int DepartmentManagerId { get; set; }

    public int PersonId { get; set; }
    public decimal? Salary { get; set; }

    public virtual DepartmentManager DepartmentManager { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<TeacherSupervisor> TeacherSupervisors { get; set; } = new List<TeacherSupervisor>();
}
