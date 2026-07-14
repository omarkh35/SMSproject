using BLL.EntitiesDTOS.DepartmentManager;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class DepartmentManagerService : IDepartmentManagerService
    {
        private readonly IBaseRepositories<ClassroomStudent> _classStudentRepo;
        private readonly IBaseRepositories<ClassroomTeacher> _classTeacherRepo;
        private readonly IBaseRepositories<TeacherSupervisor> _teacherSupervisorRepo;
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<User> _userRepo;
        IBaseRepositories<StudentRecord> _studentRecordRepo;
        IBaseRepositories<StudentParent> _studentParentRepo;
        IBaseRepositories<Mark> _markRepo;

        public DepartmentManagerService(
            IBaseRepositories<ClassRoom> classRoomRepo,
            IBaseRepositories<ClassroomStudent> classStudentRepo,
            IBaseRepositories<ClassroomTeacher> classTeacherRepo,
            IBaseRepositories<TeacherSupervisor> teacherSupervisorRepo,
            IBaseRepositories<Teacher> teacherRepo,
            IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<User> userRepo,
            IBaseRepositories<StudentRecord> studentRecordRepo,
            IBaseRepositories<Mark> markRepo,
            IBaseRepositories<StudentParent> studentParentRepo
        )
        {
            _classRoomRepo = classRoomRepo;
            _classStudentRepo = classStudentRepo;
            _classTeacherRepo = classTeacherRepo;
            _teacherSupervisorRepo = teacherSupervisorRepo;
            _teacherRepo = teacherRepo;
            _supervisorRepo = supervisorRepo;
            _personRepo = personRepo;
            _userRepo = userRepo;
            _studentRecordRepo = studentRecordRepo;
            _markRepo = markRepo;
            _studentParentRepo = studentParentRepo;
        }

        public async Task<IEnumerable<ClassRoomDto>> GetAllClassRoomsAsync()
        {
            var classes = await _classRoomRepo.GetAllWithIncludeAsync(c => c.ClassroomStudents);

            return classes.Select(c => new ClassRoomDto
            {
                Id = c.ClassRoomId,
                GradeId = c.GradeId,
                Section = c.Section,
                SupervisorId = c.SupervisorId,
                StartYear = c.StartYear,
                CurrentStudentsCount = c.ClassroomStudents.Count
            });
        }

        public async Task<ClassRoomDto> GetClassRoomByIdAsync(int id)
        {
            var c = await _classRoomRepo.GetByIdAsync(id);
            if (c == null) return null;

            return new ClassRoomDto
            {
                Id = c.ClassRoomId,
                GradeId = c.GradeId,
                Section = c.Section,
                SupervisorId = c.SupervisorId,
                StartYear = c.StartYear
            };
        }

        public async Task<ClassRoomDto> CreateClassRoomAsync(ClassRoomCreateDto dto)
        {
            var newClass = new ClassRoom
            {
                GradeId = dto.GradeId,
                Section = dto.Section,
                StartYear = dto.StartYear,
                SupervisorId = dto.SupervisorId
            };
            await _classRoomRepo.AddAsync(newClass);
            await _classRoomRepo.SaveChangesAsync();

            return new ClassRoomDto
            {
                Id = newClass.ClassRoomId,
                GradeId = newClass.GradeId,
                Section = newClass.Section,
                StartYear = newClass.StartYear
            };
        }

        public async Task<bool> UpdateClassRoomAsync(int id, ClassRoomUpdateDto dto)
        {
            var existing = await _classRoomRepo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.StartYear = dto.StartYear ?? existing.StartYear;

            _classRoomRepo.UpdateAsync(existing);
            await _classRoomRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClassRoomAsync(int id)
        {
            // هون لازم نتحقق انو مدير القسم هو المسؤول عن الصف 
            var existing = await _classRoomRepo.GetByIdAsync(id);
            if (existing == null) return false;

            _classRoomRepo.Delete(existing);
            await _classRoomRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignStudentToClassAsync(StudentToClassDto dto)
        {
            var exists = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == dto.StudentId && cs.ClassRoomId == dto.ClassRoomId
            );
            if (exists.Any()) return false;

            var link = new ClassroomStudent
            {
                StudentId = dto.StudentId,
                ClassRoomId = dto.ClassRoomId
            };

            await _classStudentRepo.AddAsync(link);
            await _classStudentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTeacherToClassAsync(TeacherToClassDto dto)
        {
            var link = new ClassroomTeacher
            {
                TeacherId = dto.TeacherId,
                ClassRoomId = dto.ClassRoomId,
                SubjectId = dto.SubjectId
            };

            await _classTeacherRepo.AddAsync(link);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignSupervisorToTeacherAsync(TeacherSupervisorDto dto)
        {
            var link = new TeacherSupervisor
            {
                SupervisorId = dto.SupervisorId,
                TeacherId = dto.TeacherId
            };

            await _teacherSupervisorRepo.AddAsync(link);
            await _teacherSupervisorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int studentId, int classRoomId)
        {
            var links = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == studentId && cs.ClassRoomId == classRoomId
            );
            var link = links.FirstOrDefault();
            if (link == null) return false;

            _classStudentRepo.Delete(link);
            await _classStudentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTeacherAssignmentAsync(TeacherToClassDto dto)
        {

            var assignment = await _classTeacherRepo.GetByIdAsync(dto.ClassroomTeacherId);

            if (assignment == null) return false;

            assignment.ClassRoomId = dto.ClassRoomId;
            assignment.SubjectId = dto.SubjectId;
            assignment.TeacherId = dto.TeacherId;

            _classTeacherRepo.UpdateAsync(assignment);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTeacherFromClassAsync(int teacherId, int classRoomId)
        {
            var assignments = await _classTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacherId && ct.ClassRoomId == classRoomId
            );

            var assignment = assignments.FirstOrDefault();
            if (assignment == null) return false;

            _classTeacherRepo.Delete(assignment);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSupervisorFromTeacherAsync(int supervisorId, int teacherId)
        {
            var relations = await _teacherSupervisorRepo.GetAllWithIncludeAndFilterAsync(
                ts => ts.SupervisorId == supervisorId && ts.TeacherId == teacherId
            );

            var relation = relations.FirstOrDefault();
            if (relation == null) return false;

            _teacherSupervisorRepo.Delete(relation);
            await _teacherSupervisorRepo.SaveChangesAsync();
            return true;
        }


        ////////////////////////////////////////////
        public async Task<SupervisorsDashboardDto> GetSupervisorsDashboardAsync(int managerPersonId)
        {
            var dashboard = new SupervisorsDashboardDto();

            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);

            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(
                s => s.Person,
                s => s.DepartmentManager,
                s => s.Person.Users
            );

            var activeSupervisorsForManager = allSupervisors
                .Where(s => s.DepartmentManager.PersonId == managerPersonId)
                .ToList();

            dashboard.TotalActiveSupervisors = activeSupervisorsForManager.Count(s => s.Person.IsActive);

            dashboard.AssignedSectionsCount = allClassRooms
                .Count(cr => cr.SupervisorId != null && activeSupervisorsForManager.Any(s => s.SupervisorId == cr.SupervisorId));

            dashboard.UnassignedClassesCount = allClassRooms.Count(cr => cr.SupervisorId == null);

            foreach (var sup in activeSupervisorsForManager)
            {
                var assignedClasses = allClassRooms
                    .Where(cr => cr.SupervisorId == sup.SupervisorId)
                    .Select(cr => $"Grade {cr.Grade.GradeNumber} (Sec {cr.Section})")
                    .Distinct()
                    .ToList();

                var associatedUserAccount = sup.Person.Users.FirstOrDefault();
                var extractedPhoneNumber = associatedUserAccount?.PhoneNumber ?? "No Active Number";

                dashboard.Supervisors.Add(new SupervisorGridItemDto
                {
                    SupervisorID = sup.SupervisorId,
                    FullName = $"{sup.Person.FirstName} {sup.Person.LastName}",
                    ProfessionalTitle = "Academic Supervisor",
                    PhoneNumber = extractedPhoneNumber,
                    Status = sup.Person.IsActive ? "Active" : "Inactive",
                    AssignedClasses = assignedClasses
                });
            }

            dashboard.TotalCount = dashboard.Supervisors.Count;

            return dashboard;
        }

        public async Task<TeachersDashboardDto> GetTeachersManagementDashboardAsync(int managerPersonId, int page)
        {
            var dashboard = new TeachersDashboardDto();
            const int pageSize = 8;

            var allTeachers = await _teacherRepo.GetAllWithIncludeAsync(
                t => t.Person,
                t => t.Person.Users
            );

            var relevantTeachers = allTeachers.Where(t => t.Person.IsActive == true).ToList();

            dashboard.TotalTeachersCount = relevantTeachers.Count;

            double averageClasses = relevantTeachers.Any() ? relevantTeachers.Average(t => t.WeeklyClasses ?? 0) : 0;
            dashboard.AvgWorkingHours = $"{(int)Math.Round(averageClasses * 0.75)}h/week";

            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalTeachersCount / pageSize);

            var paginatedTeachers = relevantTeachers
                .OrderBy(t => t.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var teacher in paginatedTeachers)
            {
                var associatedUser = teacher.Person.Users.FirstOrDefault();
                string cleanFullName = $"{teacher.Person.FirstName} {teacher.Person.SecondName} {teacher.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Teachers.Add(new TeacherGridItemDto
                {
                    TeacherID = teacher.TeacherId,
                    FullName = cleanFullName,
                    PhoneNumber = associatedUser?.PhoneNumber ?? "No Number",
                    WorkingHours = $"{(teacher.WeeklyClasses ?? 0) * 0.75}h / Week"
                });
            }

            return dashboard;
        }

        public async Task<bool> RegisterTeacherWorkflowAsync(CreateTeacherDto dto)
        {
            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                var newPerson = new Person
                {
                    FirstName = dto.FirstName,
                    SecondName = dto.SecondName,
                    LastName = dto.LastName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync();

                string? hashedPassword = null;
                if (!string.IsNullOrEmpty(dto.ClearTextPassword))
                {
                    hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.ClearTextPassword);
                }

                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 2,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    HashPassword = hashedPassword,
                    AccountNumber = generatedAccountNumber
                };

                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                var newTeacher = new Teacher
                {
                    PersonId = newPerson.PersonId,
                    WeeklyClasses = dto.WeeklyClasses,
                    SalaryPerClass = dto.SalaryPerClass
                };

                await _teacherRepo.AddAsync(newTeacher);
                await _teacherRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _classRoomRepo.RollbackTransactionAsync();
                throw new Exception($"Database Workflow Crash: {ex.Message} -> Inner: {ex.InnerException?.Message}");

                //return false;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public async Task<StudentDirectoryDashboardDto> GetStudentDirectoryDashboardAsync(int managerPersonId, int page)
        {
            var dashboard = new StudentDirectoryDashboardDto();
            const int pageSize = 8;

            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.DepartmentManager);
            var activeSupervisorIds = allSupervisors
                .Where(s => s.DepartmentManager.PersonId == managerPersonId)
                .Select(s => s.SupervisorId)
                .ToList();

            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var managedClassRooms = allClassRooms
                .Where(cr => cr.SupervisorId != null && activeSupervisorIds.Contains(cr.SupervisorId.Value))
                .ToList();
            var managedClassRoomIds = managedClassRooms.Select(cr => cr.ClassRoomId).ToList();

            var allClassroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.ClassRoom);
            var managedStudentIds = allClassroomStudents
                .Where(cs => managedClassRoomIds.Contains(cs.ClassRoomId))
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToList();

            var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
            var relevantStudentRecords = allStudentRecords
                .Where(sr => managedStudentIds.Contains(sr.StudentId))
                .ToList();

            dashboard.TotalStudentsCount = relevantStudentRecords.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.ExamType);
            var sem1Exams = allMarks
                .Where(m => m.IsApproved && m.ExamType.Semester == 1 && managedStudentIds.Contains(m.StudentRecordId))
                .ToList();

            if (sem1Exams.Any())
            {
                int passingMarks = sem1Exams.Count(m => m.MarkValue >= (m.FullMark / 2));
                double percentage = ((double)passingMarks / sem1Exams.Count) * 100;
                dashboard.PassRate = $"{percentage:F1}%";
            }
            else
            {
                dashboard.PassRate = "N/A";
            }

            var paginatedRecords = relevantStudentRecords
                .OrderBy(sr => sr.Student.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var allStudentParents = await _studentParentRepo.GetAllWithIncludeAsync(sp => sp.Parent, sp => sp.Parent.Person, sp => sp.Parent.Person.Users);

            foreach (var record in paginatedRecords)
            {
                var assignedClassLink = allClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
                string classDisplay = "Not Assigned";

                if (assignedClassLink != null)
                {
                    var matchedRoom = managedClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        classDisplay = $"Grade {matchedRoom.Grade.GradeNumber} - Section {matchedRoom.Section}";
                    }
                }

                var parentLink = allStudentParents.FirstOrDefault(sp => sp.StudentId == record.StudentId);
                var parentUserAccount = parentLink?.Parent?.Person?.Users?.FirstOrDefault();
                string parentPhone = parentUserAccount?.PhoneNumber ?? "No Parent Link";

                string cleanFullName = $"{record.Student.Person.FirstName} {record.Student.Person.SecondName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Students.Add(new StudentGridItemDto
                {
                    StudentID = record.StudentId,
                    FullName = cleanFullName,
                    ClassAndSection = classDisplay,
                    ParentPhoneNumber = parentPhone
                });
            }

            return dashboard;
        }

    }
}
