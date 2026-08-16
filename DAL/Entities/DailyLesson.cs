using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
    [ForeignKey("ClassRoomID")]
    public virtual ClassRoom ClassRoom { get; set; } = null!;
    [ForeignKey("SubjectID")]
    public virtual Subject Subject { get; set; } = null!;
    [ForeignKey("TeacherPersonID")]
    public virtual Person TeacherPerson { get; set; } = null!;
}
