using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class ClassroomStudent
{
    public int ClassroomStudentId { get; set; }

    public int StudentId { get; set; }

    public int ClassRoomId { get; set; }

    public virtual ClassRoom ClassRoom { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
