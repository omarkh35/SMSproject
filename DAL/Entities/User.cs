using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string SecondName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Male=0,Female=1
    /// </summary>
    public bool Gendor { get; set; }

    public string? HashPassword { get; set; }

    /// <summary>
    /// &apos;Student&apos;, &apos;Teacher&apos;, &apos;Manager&apos;, &apos;Admin&apos;,&apos;Supervisor&apos;
    /// </summary>
    public string UserRole { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<DepartmentManager> DepartmentManagers { get; set; } = new List<DepartmentManager>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();
}
