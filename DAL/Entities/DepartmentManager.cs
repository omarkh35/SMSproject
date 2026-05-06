using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class DepartmentManager
{
    public int DepartmentManagerId { get; set; }

    public int UserId { get; set; }

    public decimal? Salary { get; set; }

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();

    public virtual User User { get; set; } = null!;
}
