using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class Student
{
    public int StudentId { get; set; }

   

    public string MotherName { get; set; } = null!;

   
    public string Address { get; set; } = null!;

   
    public string? Picture { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();

    public virtual ICollection<StudentRecord> StudentRecords { get; set; } = new List<StudentRecord>();

    public virtual User User { get; set; } = null!;
}
