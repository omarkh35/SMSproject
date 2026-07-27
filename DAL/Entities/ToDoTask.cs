using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ToDoTask
{
    public long TaskID { get; set; }

    public int AssignedPersonID { get; set; }

    public string TaskDescription { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public int? ClassRoomID { get; set; }

    /// <summary>
    /// 1 = Normal, 2 = Mid, 3 = Important
    /// </summary>
    public byte PriorityLevel { get; set; }

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Person AssignedPerson { get; set; } = null!;

    public virtual ClassRoom? ClassRoom { get; set; }
}
