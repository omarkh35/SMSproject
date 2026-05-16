using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public string? Title { get; set; }

    public string ImagePath { get; set; } = null!;

    /// <summary>
    /// 1 = ClassRoom, 2 = Teacher
    /// </summary>
    public byte ScheduleType { get; set; }

    /// <summary>
    /// Type 1 ClassRoomID , Type 2 TeacherID
    /// </summary>
    public int ReferenceId { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
