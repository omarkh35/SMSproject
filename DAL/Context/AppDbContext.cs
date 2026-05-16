using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accountant> Accountants { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<AnnouncementClassroom> AnnouncementClassrooms { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<ClassPayment> ClassPayments { get; set; }

    public virtual DbSet<ClassRoom> ClassRooms { get; set; }

    public virtual DbSet<ClassroomStudent> ClassroomStudents { get; set; }

    public virtual DbSet<ClassroomTeacher> ClassroomTeachers { get; set; }

    public virtual DbSet<DepartmentManager> DepartmentManagers { get; set; }

    public virtual DbSet<ExamSchedule> ExamSchedules { get; set; }

    public virtual DbSet<ExamType> ExamTypes { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeSubject> GradeSubjects { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<SchoolSetting> SchoolSettings { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAttendance> StudentAttendances { get; set; }

    public virtual DbSet<StudentNote> StudentNotes { get; set; }

    public virtual DbSet<StudentParent> StudentParents { get; set; }

    public virtual DbSet<StudentRecord> StudentRecords { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SuperAdmin> SuperAdmins { get; set; }

    public virtual DbSet<Supervisor> Supervisors { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<TeacherAttendance> TeacherAttendances { get; set; }

    public virtual DbSet<TeacherSupervisor> TeacherSupervisors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=SMSDB;Trusted_Connection=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accountant>(entity =>
        {
            entity.Property(e => e.AccountantId).HasColumnName("AccountantID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.Salary).HasColumnType("money");

            entity.HasOne(d => d.Person).WithMany(p => p.Accountants)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accountants_People");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.Property(e => e.AnnouncementId).HasColumnName("AnnouncementID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsGeneral).HasComment("1 = Entire School, 0 = Target Specific");
            entity.Property(e => e.SenderPersonId).HasColumnName("SenderPersonID");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.SenderPerson).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.SenderPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Announcements_People");
        });

        modelBuilder.Entity<AnnouncementClassroom>(entity =>
        {
            entity.Property(e => e.AnnouncementClassroomId).HasColumnName("AnnouncementClassroomID");
            entity.Property(e => e.AnnouncementId).HasColumnName("AnnouncementID");
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");

            entity.HasOne(d => d.Announcement).WithMany(p => p.AnnouncementClassrooms)
                .HasForeignKey(d => d.AnnouncementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnnounceClass_Announce");

            entity.HasOne(d => d.ClassRoom).WithMany(p => p.AnnouncementClassrooms)
                .HasForeignKey(d => d.ClassRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnnounceClass_ClassRooms");
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasIndex(e => new { e.StudentFocusId, e.SupervisorPersonId, e.ParentPersonId }, "UQ_SingleChatPerChild").IsUnique();

            entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoomID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastMessageContent).HasMaxLength(255);
            entity.Property(e => e.ParentPersonId).HasColumnName("ParentPersonID");
            entity.Property(e => e.StudentFocusId).HasColumnName("StudentFocusID");
            entity.Property(e => e.SupervisorPersonId).HasColumnName("SupervisorPersonID");

            entity.HasOne(d => d.ParentPerson).WithMany(p => p.ChatRoomParentPeople)
                .HasForeignKey(d => d.ParentPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatRooms_Parent");

            entity.HasOne(d => d.StudentFocus).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.StudentFocusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatRooms_Students");

            entity.HasOne(d => d.SupervisorPerson).WithMany(p => p.ChatRoomSupervisorPeople)
                .HasForeignKey(d => d.SupervisorPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatRooms_Supervisor");
        });

        modelBuilder.Entity<ClassPayment>(entity =>
        {
            entity.ToTable("ClassPayment");

            entity.Property(e => e.ClassPaymentId).HasColumnName("ClassPaymentID");
            entity.Property(e => e.FullAmount).HasColumnType("money");
        });

        modelBuilder.Entity<ClassRoom>(entity =>
        {
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");

            entity.HasOne(d => d.Grade).WithMany(p => p.ClassRooms)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassRooms_Grades");
        });

        modelBuilder.Entity<ClassroomStudent>(entity =>
        {
            entity.ToTable("ClassroomStudent");

            entity.Property(e => e.ClassroomStudentId).HasColumnName("ClassroomStudentID");
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.ClassRoom).WithMany(p => p.ClassroomStudents)
                .HasForeignKey(d => d.ClassRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassroomStudent_ClassRooms");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassroomStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassroomStudent_Students");
        });

        modelBuilder.Entity<ClassroomTeacher>(entity =>
        {
            entity.ToTable("ClassroomTeacher");

            entity.Property(e => e.ClassroomTeacherId).HasColumnName("ClassroomTeacherID");
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.ClassRoom).WithMany(p => p.ClassroomTeachers)
                .HasForeignKey(d => d.ClassRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassroomTeacher_ClassRooms");

            entity.HasOne(d => d.Subject).WithMany(p => p.ClassroomTeachers)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassroomTeacher_Subjects");

            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassroomTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassroomTeacher_Teachers");
        });

        modelBuilder.Entity<DepartmentManager>(entity =>
        {
            entity.Property(e => e.DepartmentManagerId).HasColumnName("DepartmentManagerID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.Salary).HasColumnType("money");

            entity.HasOne(d => d.Person).WithMany(p => p.DepartmentManagers)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DepartmentManagers_People");
        });

        modelBuilder.Entity<ExamSchedule>(entity =>
        {
            entity.Property(e => e.ExamScheduleId).HasColumnName("ExamScheduleID");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Grade).WithMany(p => p.ExamSchedules)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamSchedules_Grades");
        });

        modelBuilder.Entity<ExamType>(entity =>
        {
            entity.Property(e => e.ExamTypeId).HasColumnName("ExamTypeID");
            entity.Property(e => e.ExamTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK_Classes");

            entity.Property(e => e.GradeId)
                .ValueGeneratedNever()
                .HasColumnName("GradeID");
        });

        modelBuilder.Entity<GradeSubject>(entity =>
        {
            entity.HasKey(e => e.GradeSubjectId).HasName("PK_Class_Subject");

            entity.ToTable("GradeSubject");

            entity.Property(e => e.GradeSubjectId).HasColumnName("GradeSubjectID");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.Grade).WithMany(p => p.GradeSubjects)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Class_Subject_Classes");

            entity.HasOne(d => d.Subject).WithMany(p => p.GradeSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GradeSubject_Subjects");
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.Property(e => e.MarkId).HasColumnName("MarkID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ExamTypeId).HasColumnName("ExamTypeID");
            entity.Property(e => e.FullMark).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsApproved).HasComment("0 = Pending Review, 1 = Live to Parents");
            entity.Property(e => e.MarkValue).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.StudentRecordId).HasColumnName("StudentRecordID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.ExamType).WithMany(p => p.Marks)
                .HasForeignKey(d => d.ExamTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Marks_ExamTypes");

            entity.HasOne(d => d.StudentRecord).WithMany(p => p.Marks)
                .HasForeignKey(d => d.StudentRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Marks_StudentRecords");

            entity.HasOne(d => d.Subject).WithMany(p => p.Marks)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Marks_Subjects");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.ChatRoomId).HasColumnName("ChatRoomID");
            entity.Property(e => e.SenderPersonId).HasColumnName("SenderPersonID");
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ChatRoom).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Rooms");

            entity.HasOne(d => d.SenderPerson).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Sender");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.PaymentAmount).HasColumnType("money");
            entity.Property(e => e.StudentRecordId).HasColumnName("StudentRecordID");

            entity.HasOne(d => d.StudentRecord).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StudentRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_StudentRecords");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.SecondName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.ReferenceId)
                .HasComment("Type 1 ClassRoomID , Type 2 TeacherID")
                .HasColumnName("ReferenceID");
            entity.Property(e => e.ScheduleType).HasComment("1 = ClassRoom, 2 = Teacher");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<SchoolSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId);

            entity.Property(e => e.SettingId).HasColumnName("SettingID");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SchoolLogo).HasMaxLength(255);
            entity.Property(e => e.SchoolName).HasMaxLength(100);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MotherName).HasMaxLength(30);
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.Picture).HasMaxLength(100);

            entity.HasOne(d => d.Person).WithMany(p => p.Students)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_People");
        });

        modelBuilder.Entity<StudentAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);

            entity.ToTable("StudentAttendance");

            entity.HasIndex(e => new { e.StudentId, e.AttendanceDate }, "UQ_StudentDaily").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.Status).HasComment("1=Present, 2=Absent, 3=Late, 4=Excused");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ClassRoom).WithMany(p => p.StudentAttendances)
                .HasForeignKey(d => d.ClassRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendance_ClassRooms");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAttendances)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendance_Students");
        });

        modelBuilder.Entity<StudentNote>(entity =>
        {
            entity.HasKey(e => e.NoteId);

            entity.Property(e => e.NoteId).HasColumnName("NoteID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TeacherPersonId).HasColumnName("TeacherPersonID");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentNotes)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentNotes_Students");

            entity.HasOne(d => d.TeacherPerson).WithMany(p => p.StudentNotes)
                .HasForeignKey(d => d.TeacherPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentNotes_People");
        });

        modelBuilder.Entity<StudentParent>(entity =>
        {
            entity.Property(e => e.StudentParentId)
                .ValueGeneratedNever()
                .HasColumnName("StudentParentID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.RelationshipType).HasMaxLength(50);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Person).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentParents_People");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentParents_Students");
        });

        modelBuilder.Entity<StudentRecord>(entity =>
        {
            entity.Property(e => e.StudentRecordId).HasColumnName("StudentRecordID");
            entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.YearlyPayment).HasColumnType("money");

            entity.HasOne(d => d.Grade).WithMany(p => p.StudentRecords)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentRecords_Grades");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentRecords)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentRecords_Students");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.SubjectName).HasMaxLength(50);
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.AdminId);

            entity.ToTable("SuperAdmin");

            entity.HasIndex(e => e.Username, "UQ_AdminUsername").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.StaticPassword).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");
            entity.Property(e => e.DepartmentManagerId).HasColumnName("DepartmentManagerID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.Salary).HasColumnType("money");

            entity.HasOne(d => d.DepartmentManager).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.DepartmentManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Supervisors_DepartmentManagers");

            entity.HasOne(d => d.Person).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Supervisors_People");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.SalaryPerClass).HasColumnType("money");

            entity.HasOne(d => d.Person).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teachers_People");
        });

        modelBuilder.Entity<TeacherAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);

            entity.ToTable("TeacherAttendance");

            entity.HasIndex(e => new { e.TeacherId, e.AttendanceDate }, "UQ_TeacherDaily").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherAttendances)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherAttendance_Teachers");
        });

        modelBuilder.Entity<TeacherSupervisor>(entity =>
        {
            entity.ToTable("TeacherSupervisor");

            entity.Property(e => e.TeacherSupervisorId).HasColumnName("TeacherSupervisorID");
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.TeacherSupervisors)
                .HasForeignKey(d => d.SupervisorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherSupervisor_Supervisors");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherSupervisors)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherSupervisor_Teachers");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AccountNumber).HasMaxLength(8);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.PersonId).HasColumnName("PersonID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");

            entity.HasOne(d => d.Person).WithMany(p => p.Users)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_People");

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__UserRefr__F5845E59E232063D");

            entity.Property(e => e.RefreshTokenId).HasColumnName("RefreshTokenID");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
