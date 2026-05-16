using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class TeacherAttendance
{
    public int AttendanceId { get; set; }

    public int TeacherId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public byte Status { get; set; }

    public string? Notes { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Teacher Teacher { get; set; } = null!;
}
