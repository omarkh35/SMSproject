using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public string MotherName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Picture { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int PersonId { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();

    public virtual ICollection<StudentNote> StudentNotes { get; set; } = new List<StudentNote>();

    public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();

    public virtual ICollection<StudentRecord> StudentRecords { get; set; } = new List<StudentRecord>();
}
