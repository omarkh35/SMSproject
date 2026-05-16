using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Person
{
    public int PersonId { get; set; }

    public string FirstName { get; set; } = null!;

    public string SecondName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public bool Gender { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Accountant> Accountants { get; set; } = new List<Accountant>();

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<ChatRoom> ChatRoomParentPeople { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ChatRoom> ChatRoomSupervisorPeople { get; set; } = new List<ChatRoom>();

    public virtual ICollection<DepartmentManager> DepartmentManagers { get; set; } = new List<DepartmentManager>();

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<StudentNote> StudentNotes { get; set; } = new List<StudentNote>();

    public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
