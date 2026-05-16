using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class StudentNote
{
    public long NoteId { get; set; }

    public int StudentId { get; set; }

    public int TeacherPersonId { get; set; }

    public string NoteContent { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Person TeacherPerson { get; set; } = null!;
}
