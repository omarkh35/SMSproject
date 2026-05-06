using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SMS.Entities;

public partial class SmsdbContext : DbContext
{
    public SmsdbContext()
    {
    }

    public SmsdbContext(DbContextOptions<SmsdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClassPayment> ClassPayments { get; set; }

    public virtual DbSet<ClassRoom> ClassRooms { get; set; }

    public virtual DbSet<ClassroomStudent> ClassroomStudents { get; set; }

    public virtual DbSet<ClassroomTeacher> ClassroomTeachers { get; set; }

    public virtual DbSet<DepartmentManager> DepartmentManagers { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentRecord> StudentRecords { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Supervisor> Supervisors { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<TeacherSupervisor> TeacherSupervisors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=SMSDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassPayment>(entity =>
        {
            entity.ToTable("ClassPayment");

            entity.Property(e => e.ClassPaymentId).HasColumnName("ClassPaymentID");
            entity.Property(e => e.FullAmount).HasColumnType("money");
        });

        modelBuilder.Entity<ClassRoom>(entity =>
        {
            entity.Property(e => e.ClassRoomId).HasColumnName("ClassRoomID");
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.ClassRooms)
                .HasForeignKey(d => d.SupervisorId)
                .HasConstraintName("FK_ClassRooms_Supervisors");
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
            entity.Property(e => e.Salary).HasColumnType("money");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.DepartmentManagers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DepartmentManagers_Users");
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

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.MotherName).HasMaxLength(30);
            entity.Property(e => e.Picture).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<StudentRecord>(entity =>
        {
            entity.Property(e => e.StudentRecordId).HasColumnName("StudentRecordID");
            entity.Property(e => e.CreatedAt).HasColumnName("Created_At");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.StudentNumber).HasMaxLength(10);
            entity.Property(e => e.YearlyPayment).HasColumnType("money");

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

        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");
            entity.Property(e => e.DepartmentManagerId).HasColumnName("DepartmentManagerID");
            entity.Property(e => e.Salary).HasColumnType("money");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.DepartmentManager).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.DepartmentManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Supervisors_DepartmentManagers");

            entity.HasOne(d => d.User).WithMany(p => p.Supervisors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Supervisors_Users");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.SalaryPerClass).HasColumnType("money");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Teachers_Users");
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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gendor).HasComment("Male=0,Female=1");
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.SecondName).HasMaxLength(50);
            entity.Property(e => e.UserRole)
                .HasMaxLength(20)
                .HasDefaultValue("Student")
                .HasComment("'Student', 'Teacher', 'Manager', 'Admin','Supervisor'");
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
