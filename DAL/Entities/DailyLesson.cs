using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class DailyLesson
{
    public long DailyLessonID { get; set; }

    public int ClassRoomID { get; set; }

    public int SubjectID { get; set; }

    public int TeacherPersonID { get; set; }

    public DateOnly LessonDate { get; set; }

    public string Description { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ClassRoom ClassRoom { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person TeacherPerson { get; set; } = null!;
}
