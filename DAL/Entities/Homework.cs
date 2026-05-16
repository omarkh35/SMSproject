using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Homework
{
    public int HomeworkId { get; set; }

    public int ClassRoomId { get; set; }

    public int SubjectId { get; set; }

    public int TeacherPersonId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? AttachmentPath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ClassRoom ClassRoom { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person TeacherPerson { get; set; } = null!;
}
