using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ClassRoom
{
    public int ClassRoomId { get; set; }

    public int GradeId { get; set; }

    public byte Section { get; set; }

    public int? SupervisorId { get; set; }

    public short StartYear { get; set; }

    public virtual ICollection<AnnouncementClassroom> AnnouncementClassrooms { get; set; } = new List<AnnouncementClassroom>();

    public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();

    public virtual ICollection<ClassroomTeacher> ClassroomTeachers { get; set; } = new List<ClassroomTeacher>();

    public virtual ICollection<DailyLesson> DailyLessons { get; set; } = new List<DailyLesson>();

    public virtual Grade Grade { get; set; } = null!;

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();

    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();
}
