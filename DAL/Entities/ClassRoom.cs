using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class ClassRoom
{
    public int ClassRoomId { get; set; }

    public byte Grade { get; set; }

    public byte Section { get; set; }

    public int? SupervisorId { get; set; }

    public short StartYear { get; set; }

    public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();

    public virtual ICollection<ClassroomTeacher> ClassroomTeachers { get; set; } = new List<ClassroomTeacher>();

    public virtual Supervisor? Supervisor { get; set; }
}
