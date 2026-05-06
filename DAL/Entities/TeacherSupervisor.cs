using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class TeacherSupervisor
{
    public int TeacherSupervisorId { get; set; }

    public int SupervisorId { get; set; }

    public int TeacherId { get; set; }

    public virtual Supervisor Supervisor { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
