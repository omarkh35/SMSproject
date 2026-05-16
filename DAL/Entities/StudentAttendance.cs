using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class StudentAttendance
{
    public int AttendanceId { get; set; }

    public int StudentId { get; set; }

    public int ClassRoomId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    /// <summary>
    /// 1=Present, 2=Absent, 3=Late, 4=Excused
    /// </summary>
    public byte Status { get; set; }

    public string? Notes { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ClassRoom ClassRoom { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
