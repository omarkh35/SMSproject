using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ClassroomTeacher
{
    public int ClassroomTeacherId { get; set; }

    public int ClassRoomId { get; set; }

    public int TeacherId { get; set; }

    public int SubjectId { get; set; }

    public virtual ClassRoom ClassRoom { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
