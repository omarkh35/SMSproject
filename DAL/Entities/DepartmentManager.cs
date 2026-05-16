using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class DepartmentManager
{
    public int DepartmentManagerId { get; set; }

    public int PersonId { get; set; }

    public decimal? Salary { get; set; }

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}
