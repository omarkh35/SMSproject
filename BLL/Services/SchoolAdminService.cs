using BLL.EntitiesDTOS.SchoolAdmin;
using DAL.Entities;
using DAL.Interfaces;
using BLL.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class SchoolAdminService : ISchoolAdminService
    {
        private readonly IBaseRepositories<Subject> _subjectRepo;
        private readonly IBaseRepositories<GradeSubject> _gradeSubjectRepo;
        private readonly IBaseRepositories<DepartmentManager> _managerRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IBaseRepositories<Grade> _gradeRepo;
        private readonly IBaseRepositories<StudentRecord> _studentRecordRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;
        private readonly IBaseRepositories<ClassroomTeacher> _classTeacherRepo;
        private readonly IBaseRepositories<Mark> _markRepo;
        private readonly IBaseRepositories<Announcement> _announcementRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classStudentRepo;
        private readonly IBaseRepositories<ExamSchedule> _examScheduleRepo;
        private readonly IBaseRepositories<ClassPayment> _classPaymentRepo;
        private readonly IBaseRepositories<Accountant> _accountantRepo;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;

        public SchoolAdminService(
            IBaseRepositories<Subject> subjectRepo,
            IBaseRepositories<GradeSubject> gradeSubjectRepo,
            IBaseRepositories<DepartmentManager> managerRepo,
            IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<User> userRepo,
            IBaseRepositories<ClassRoom> classRoomRepo, IBaseRepositories<Grade> gradeRepo,
        IBaseRepositories<StudentRecord> studentRecordRepo,
        IBaseRepositories<Teacher> teacherRepo,
        IBaseRepositories<ClassroomTeacher> classTeacherRepo,
        IBaseRepositories<Mark> markRepo,
        IBaseRepositories<Announcement> announcementRepo,
        IBaseRepositories<ClassroomStudent> classStudentRepo,
        IBaseRepositories<ExamSchedule> examScheduleRepo,
        IBaseRepositories<ClassPayment> classPaymentRepo,
        IBaseRepositories<Accountant> accountantRepo, IFileService fileService,
        IEmailService emailService)
        {
            _subjectRepo = subjectRepo;
            _gradeSubjectRepo = gradeSubjectRepo;
            _managerRepo = managerRepo;
            _supervisorRepo = supervisorRepo;
            _personRepo = personRepo;
            _userRepo = userRepo;
            _classRoomRepo = classRoomRepo;
            _gradeRepo = gradeRepo;
            _markRepo = markRepo;
            _announcementRepo = announcementRepo;
            _studentRecordRepo = studentRecordRepo;
            _teacherRepo = teacherRepo;
            _classStudentRepo = classStudentRepo;
            _examScheduleRepo = examScheduleRepo;
            _classTeacherRepo = classTeacherRepo;
            _accountantRepo = accountantRepo;
            _classPaymentRepo = classPaymentRepo;
            _fileService = fileService;
            _emailService = emailService;
        }

       
        public async Task<SubjectDto> CreateSubjectAsync(SubjectCreateDto dto)
        {
            var subject = new Subject
            {
                SubjectName = dto.SubjectName.Trim().ToUpper()
            };
            await _subjectRepo.AddAsync(subject);
            await _subjectRepo.SaveChangesAsync();

            return new SubjectDto
            {
                Id = subject.SubjectId, 
                SubjectName = subject.SubjectName
            };
        }

        public async Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync()
        {
            var subjects = await _subjectRepo.GetAllAsync();
            return subjects.Select(s => new SubjectDto
            {
                Id = s.SubjectId,
                SubjectName = s.SubjectName
            });
        }

        public async Task<bool> UpdateSubjectAsync(int id, SubjectUpdateDto dto)
        {
            var subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null) return false;

            subject.SubjectName = dto.SubjectName.Trim().ToUpper();
            _subjectRepo.UpdateAsync(subject);
            await _subjectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            var subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null) return false;

            _subjectRepo.Delete(subject);
            await _subjectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<GradeSubjectDto> AssignSubjectToGradeAsync(GradeSubjectDto dto)
        {
            var gradeSubject = new GradeSubject
            {
                GradeId = dto.GradeId,
                SubjectId = dto.SubjectId
            };

            await _gradeSubjectRepo.AddAsync(gradeSubject);
            await _gradeSubjectRepo.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> RemoveSubjectFromGradeAsync(int gradeId, int subjectId)
        {
            var links = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(
                gs => gs.GradeId == gradeId && gs.SubjectId == subjectId
            );

            var link = links.FirstOrDefault();
            if (link == null) return false;

            _gradeSubjectRepo.Delete(link);
            await _gradeSubjectRepo.SaveChangesAsync();
            return true;
        }

      
        public async Task<IEnumerable<StaffDto>> GetAllDepartmentManagersAsync()
        {
            var managers = await _managerRepo.GetAllWithIncludeAsync(m => m.Person);
            var allUsers = await _userRepo.GetAllAsync();

            return managers.Select(m =>
            {
                var associatedUser = allUsers.FirstOrDefault(u => u.PersonId == m.PersonId);
                return new StaffDto
                {
                    Id = m.DepartmentManagerId,
                    PersonId = m.PersonId,
                    FullName = $"{m.Person.FirstName} {m.Person.LastName}".Replace("  ", " ").Trim(),
                    Salary = m.Salary,
                    Role = "DEPARTMENT MANAGER",
                    AccountNumber = associatedUser?.AccountNumber ?? "N/A" 
                };
            }).ToList();
        }

        public async Task<StaffDto?> AddDepartmentManagerAsync(DepartmentManagerCreateDto dto)
        {
            var transaction = await _classRoomRepo.BeginTransactionAsync();
            try
            {
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); 
                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 1, 
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null, 
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                var manager = new DepartmentManager
                {
                    PersonId = newPerson.PersonId,
                    Salary = dto.Salary
                };
                await _managerRepo.AddAsync(manager);
                await _managerRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();

                try
                {
                    if (!string.IsNullOrEmpty(dto.Email))
                    {
                        await _emailService.SendUserNumberAsync(dto.Email.Trim(), generatedAccountNumber);
                    }
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[Email Service Warning] Failed to deliver SMTP numbers: {emailEx.Message}");
                }

                return new StaffDto
                {
                    Id = manager.DepartmentManagerId,
                    PersonId = manager.PersonId,
                    FullName = $"{newPerson.FirstName} {newPerson.LastName}".Trim(),
                    Salary = manager.Salary,
                    Role = "DEPARTMENT MANAGER",
                    AccountNumber = generatedAccountNumber 
                };
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return null;
            }
        }

        public async Task<bool> UpdateDepartmentManagerAsync(int id, StaffUpdateDto dto)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return false;

            manager.Salary = dto.Salary;
            _managerRepo.UpdateAsync(manager);
            await _managerRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentManagerAsync(int id)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return false;

            _managerRepo.Delete(manager);
            await _managerRepo.SaveChangesAsync();
            return true;
        }

      
        public async Task<IEnumerable<StaffDto>> GetAllSupervisorsAsync()
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.Person);
            var managers = await _managerRepo.GetAllWithIncludeAsync(m => m.Person);
            var allUsers = await _userRepo.GetAllAsync();

            return supervisors.Select(s =>
            {
                var associatedUser = allUsers.FirstOrDefault(u => u.PersonId == s.PersonId);
                var matchedManager = managers.FirstOrDefault(m => m.DepartmentManagerId == s.DepartmentManagerId);

                string managerNameStr = matchedManager != null ? $"{matchedManager.Person.FirstName} {matchedManager.Person.LastName}".Trim() : "NOT ASSIGNED"; return new StaffDto { Id = s.SupervisorId, PersonId = s.PersonId, FullName = $"{s.Person.FirstName} {s.Person.LastName}".Trim(), Salary = s.Salary, Role = "SUPERVISOR", AccountNumber = associatedUser?.AccountNumber ?? "N/A", DepartmentManagerName = managerNameStr };
            }).ToList();
        }
        public async Task<StaffDto?> AddSupervisorAsync(SupervisorCreateDto dto)
        {
            var transaction = await _classRoomRepo.BeginTransactionAsync();
            try
            {
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); 

                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 4, 
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null, 
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                var supervisor = new Supervisor
                {
                    PersonId = newPerson.PersonId,
                    Salary = dto.Salary,
                    DepartmentManagerId = dto.DepartmentManagerId
                };
                await _supervisorRepo.AddAsync(supervisor);
                await _supervisorRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();

                try
                {
                    if (!string.IsNullOrEmpty(dto.Email))
                    {
                        await _emailService.SendUserNumberAsync(dto.Email.Trim(), generatedAccountNumber);
                    }
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[Email Service Warning] Failed to deliver SMTP numbers: {emailEx.Message}");
                }

                return new StaffDto
                {
                    Id = supervisor.SupervisorId,
                    PersonId = supervisor.PersonId,
                    FullName = $"{newPerson.FirstName} {newPerson.LastName}".Trim(),
                    Salary = supervisor.Salary,
                    Role = "SUPERVISOR",
                    AccountNumber = generatedAccountNumber 
                };
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return null;
            }
        }

        public async Task<bool> UpdateSupervisorAsync(int id, StaffUpdateDto dto)
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if (supervisor == null) return false;

            supervisor.Salary = dto.Salary;
            _supervisorRepo.UpdateAsync(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSupervisorAsync(int id)
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if (supervisor == null) return false;

            _supervisorRepo.Delete(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<AdminDashboardDto> GetMainDashboardMetricsAsync()
        {
            var dashboard = new AdminDashboardDto();

            
            var allGrades = await _gradeRepo.GetAllAsync();
            var allStudentRecords = await _studentRecordRepo.GetAllAsync();
            var allTeachers = await _teacherRepo.GetAllAsync();
            var allManagers = await _managerRepo.GetAllAsync();
            var allSupervisors = await _supervisorRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllAsync();
            var allClassTeachers = await _classTeacherRepo.GetAllAsync();
            var allGradeSubjects = await _gradeSubjectRepo.GetAllAsync();
            var allMarks = await _markRepo.GetAllAsync();
            var allAnnouncements = await _announcementRepo.GetAllAsync(); 

            dashboard.TotalStudents = allStudentRecords.Select(sr => sr.StudentId).Distinct().Count();
            dashboard.TotalTeachers = allTeachers.Count();
            dashboard.TotalDepartmentManagers = allManagers.Count();
            dashboard.TotalSupervisors = allSupervisors.Count();

            var approvedMarks = allMarks.Where(m => m.IsApproved).ToList();
            if (approvedMarks.Any())
            {
                int passingCount = approvedMarks.Count(m => m.MarkValue >= (m.FullMark / 2));
                double passPercentage = ((double)passingCount / approvedMarks.Count) * 100;
                dashboard.SuccessRate = $"{passPercentage:F1}%";
            }
            else
            {
                dashboard.SuccessRate = "N/A";
            }

          
            var latestAnnouncements = allAnnouncements
                .OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue)
                .Take(10) 
                .ToList();

            foreach (var announce in latestAnnouncements)
            {
                string textSummary = announce.AnnouncementBody.Length > 40
                    ? announce.AnnouncementBody.Substring(0, 37) + "..."
                    : announce.AnnouncementBody;

                dashboard.Announcements.Add(new DashboardAnnouncementItemDto
                {
                    AnnouncementID = announce.AnnouncementId,
                    Title = announce.Title,
                    BodySummary = textSummary,
                    TargetAudience = announce.IsGeneral ? "All" : "Parents",
                    CreatedDateStr = announce.CreatedAt.HasValue
                        ? announce.CreatedAt.Value.ToString("MMM dd, yyyy")
                        : "Recent"
                });
            }

            var sortedGrades = allGrades.OrderBy(g => g.GradeNumber).ToList();

            foreach (var grade in sortedGrades)
            {
                int studentsInGrade = allStudentRecords.Count(sr => sr.GradeId == grade.GradeId);
                int sectionsInGrade = allClassRooms.Count(cr => cr.GradeId == grade.GradeId);

                if (studentsInGrade > 0 || sectionsInGrade > 0)
                {
                    dashboard.StudentsPerGrade.Add(new StudentsPerGradeGridItemDto
                    {
                        GradeName = $"Grade {grade.GradeNumber}",
                        StudentsCount = studentsInGrade,
                        SectionsCount = sectionsInGrade
                    });
                }
            }
            dashboard.TTotalStudents = dashboard.StudentsPerGrade.Sum(s => s.StudentsCount);
            dashboard.TotalSections = dashboard.StudentsPerGrade.Sum(s => s.SectionsCount);

            foreach (var grade in sortedGrades)
            {
                var roomIdsInGrade = allClassRooms.Where(cr => cr.GradeId == grade.GradeId).Select(cr => cr.ClassRoomId).ToList();
                int teachersInGrade = allClassTeachers.Where(ct => roomIdsInGrade.Contains(ct.ClassRoomId)).Select(ct => ct.TeacherId).Distinct().Count();
                int subjectsInGrade = allGradeSubjects.Count(gs => gs.GradeId == grade.GradeId);

                if (teachersInGrade > 0 || subjectsInGrade > 0)
                {
                    dashboard.TeachersPerGrade.Add(new TeachersPerGradeGridItemDto
                    {
                        GradeName = $"Grade {grade.GradeNumber}",
                        TeachersCount = teachersInGrade,
                        SubjectsCount = subjectsInGrade
                    });
                }
            }
            dashboard.TTotalTeachers = dashboard.TeachersPerGrade.Sum(t => t.TeachersCount);
            dashboard.TotalSubjects = dashboard.TeachersPerGrade.Sum(t => t.SubjectsCount);

            return dashboard;
        }



        public async Task<AdminTeachersDashboardDto> GetTeachersManagementGridAsync(string? searchName, int page)
        {
            var dashboard = new AdminTeachersDashboardDto();
            const int pageSize = 8; 

            var allTeachers = await _teacherRepo.GetAllWithIncludeAsync(t => t.Person);
            var allUsers = await _userRepo.GetAllAsync();
            var allClassTeachers = await _classTeacherRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);

            var filteredTeachers = allTeachers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredTeachers = filteredTeachers.Where(t =>
                    t.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    t.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    t.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            var matchingTeachersList = filteredTeachers.ToList();

            dashboard.TotalTeachersCount = matchingTeachersList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalTeachersCount / pageSize);

            var paginatedTeachers = matchingTeachersList
                .OrderBy(t => t.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var teacher in paginatedTeachers)
            {
                decimal calculatedSalary = (teacher.WeeklyClasses ?? 0) * (decimal)(teacher.SalaryPerClass ?? 0);

                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == teacher.PersonId);
                string phoneContact = matchedUser?.PhoneNumber ?? "No Phone Locked";

                var teacherAssignedRoomIds = allClassTeachers
                    .Where(ct => ct.TeacherId == teacher.TeacherId)
                    .Select(ct => ct.ClassRoomId)
                    .ToList();

                var assignedGrades = allClassRooms
                    .Where(cr => teacherAssignedRoomIds.Contains(cr.ClassRoomId))
                    .Select(cr => cr.Grade.GradeNumber)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                string gradesDisplayStr = assignedGrades.Any()
                    ? string.Join(", ", assignedGrades)
                    : "None";

                string combinedFullName = $"{teacher.Person.FirstName} {teacher.Person.LastName}".Replace("  ", " ").Trim();
                string statusText = teacher.Person.IsActive ? "Active" : "Inactive";

                dashboard.Teachers.Add(new AdminTeacherGridItemDto
                {
                    TeacherID = teacher.TeacherId,
                    FullName = combinedFullName,
                    Status = statusText,
                    Grades = gradesDisplayStr,
                    Salary = calculatedSalary,
                    Phone = phoneContact
                });
            }

            return dashboard;
        }



        public async Task<AdminSupervisorsDashboardDto> GetSupervisorsManagementGridAsync(string? searchName, int page)
        {
            var dashboard = new AdminSupervisorsDashboardDto();
            const int pageSize = 8; 

            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.Person);
            var allUsers = await _userRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);

            var filteredSupervisors = allSupervisors.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredSupervisors = filteredSupervisors.Where(s =>
                    s.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    s.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    s.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            var matchingSupervisorsList = filteredSupervisors.ToList();

            dashboard.TotalSupervisorsCount = matchingSupervisorsList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalSupervisorsCount / pageSize);

            var paginatedSupervisors = matchingSupervisorsList
                .OrderBy(s => s.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var supervisor in paginatedSupervisors)
            {
                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == supervisor.PersonId);
                string phoneContact = matchedUser?.PhoneNumber ?? "No Phone Locked";

                var supervisedRooms = allClassRooms
                    .Where(cr => cr.SupervisorId == supervisor.SupervisorId)
                    .ToList();

                string formattedSectionsStr = "None";

                if (supervisedRooms.Any())
                {
                    var groupedByGrade = supervisedRooms
                        .GroupBy(cr => cr.Grade.GradeNumber)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var sectionNumbers = g.Select(cr => cr.Section).OrderBy(s => s).ToList();
                            string sectionsInsideStr = string.Join(",", sectionNumbers);

                            return $"{g.Key}({sectionsInsideStr})";
                        });

                    formattedSectionsStr = string.Join(", ", groupedByGrade);
                }

                string combinedFullName = $"{supervisor.Person.FirstName} {supervisor.Person.LastName}".Replace("  ", " ").Trim();
                string statusText = supervisor.Person.IsActive ? "Active" : "Inactive";

                dashboard.Supervisors.Add(new AdminSupervisorGridItemDto
                {
                    SupervisorID = supervisor.SupervisorId,
                    FullName = combinedFullName,
                    Phone = phoneContact,
                    Status = statusText,
                    Sections = formattedSectionsStr, 
                    Salary = supervisor.Salary ?? 0
                });
            }

            return dashboard;
        }




        public async Task<AdminManagersDashboardDto> GetDepartmentManagersGridAsync(string? searchName, int page)
        {
            var dashboard = new AdminManagersDashboardDto();
            const int pageSize = 8; 

            var allManagers = await _managerRepo.GetAllWithIncludeAsync(m => m.Person);
            var allUsers = await _userRepo.GetAllAsync();

            var filteredManagers = allManagers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredManagers = filteredManagers.Where(m =>
                    m.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    m.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    m.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            var matchingList = filteredManagers.ToList();

            dashboard.TotalManagersCount = matchingList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalManagersCount / pageSize);

            var paginatedData = matchingList
                .OrderBy(m => m.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var manager in paginatedData)
            {
                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == manager.PersonId);
                string phoneContact = matchedUser?.PhoneNumber ?? "No Active Number";

                string statusText = manager.Person.IsActive ? "Active" : "Inactive";
                string combinedFullName = $"{manager.Person.FirstName} {manager.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Managers.Add(new AdminManagerGridItemDto
                {
                    DepartmentManagerID = manager.DepartmentManagerId,
                    FullName = combinedFullName,
                    Status = statusText,
                    Phone = phoneContact,
                    Salary = manager.Salary ?? 0 
                });
            }

            return dashboard;
        }


        public async Task<AdminStudentsDashboardDto> GetStudentsManagementGridAsync(string? searchName, int? gradeId, int? sectionNumber, int page)
        {
            var dashboard = new AdminStudentsDashboardDto();
            const int pageSize = 8; 

            var allGrades = await _gradeRepo.GetAllAsync();
            dashboard.AvailableGrades = allGrades.OrderBy(g => g.GradeNumber).Select(g => new GradeDropdownItemDto
            {
                GradeID = g.GradeId,
                GradeDisplayName = $"Grade {g.GradeNumber}"
            }).ToList();

            var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var allClassroomStudents = await _classStudentRepo.GetAllAsync();

            dashboard.AvailableSections = allClassRooms
    .Select(cr => (int)cr.Section)
    .Distinct()
    .OrderBy(s => s)
    .ToList();
            var fullStudentsList = new List<AdminStudentGridItemDto>();

            foreach (var record in allStudentRecords)
            {
                string gradeDisplayStr = "Not Assigned";
                int sectionDisplayNum = 0;
                int currentClassRoomId = 0;

                var assignedClassLink = allClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
                if (assignedClassLink != null)
                {
                    var matchedRoom = allClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        gradeDisplayStr = $"Grade {matchedRoom.Grade.GradeNumber}";
                        sectionDisplayNum = matchedRoom.Section; 
                        currentClassRoomId = matchedRoom.ClassRoomId;
                    }
                }

                string combinedFullName = $"{record.Student.Person.FirstName} {record.Student.Person.SecondName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                fullStudentsList.Add(new AdminStudentGridItemDto
                {
                    StudentID = record.StudentId,
                    StudentName = combinedFullName,
                    Grade = gradeDisplayStr,
                    Section = sectionDisplayNum
                });
            }

            var filteredQuery = fullStudentsList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredQuery = filteredQuery.Where(s => s.StudentName.ToLower().Contains(cleanSearch));
            }

            if (gradeId.HasValue)
            {
                var matchedGrade = allGrades.FirstOrDefault(g => g.GradeId == gradeId.Value);
                if (matchedGrade != null)
                {
                    string targetGradeStr = $"Grade {matchedGrade.GradeNumber}";
                    filteredQuery = filteredQuery.Where(s => s.Grade == targetGradeStr);
                }
            }

           
            if (gradeId.HasValue && sectionNumber.HasValue)
            {
                filteredQuery = filteredQuery.Where(s => s.Section == sectionNumber.Value);
            }

            var finalizedFilteredList = filteredQuery.ToList();

            dashboard.TotalStudentsCount = finalizedFilteredList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            dashboard.Students = finalizedFilteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return dashboard;
        }


        public async Task<GradeConfigViewDto> GetGradeConfigurationAsync(int gradeId)
        {
            var config = new GradeConfigViewDto { GradeID = gradeId };

            var allSubjects = await _subjectRepo.GetAllAsync();
            var currentLinks = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(gs => gs.GradeId == gradeId);

            
            foreach (var sub in allSubjects)
            {
                string upperName = sub.SubjectName.ToUpper();
               
                bool isAssigned = currentLinks.Any(link => link.SubjectId == sub.SubjectId);

                config.AllSubjects.Add(new SubjectConfigItemDto
                {
                    SubjectID = sub.SubjectId,
                    SubjectName = sub.SubjectName,
                    IsAssigned = isAssigned
                });
            }

            return config;
        }

        public async Task<bool> SaveGradeSubjectsConfigurationAsync(SaveGradeSubjectsDto dto)
        {
            var transaction = await _gradeSubjectRepo.BeginTransactionAsync();
            try
            {
                var existingLinks = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(gs => gs.GradeId == dto.GradeID);
                foreach (var link in existingLinks)
                {
                    _gradeSubjectRepo.Delete(link);
                }
                await _gradeSubjectRepo.SaveChangesAsync();

                if (dto.SelectedSubjectIDs != null && dto.SelectedSubjectIDs.Any())
                {
                    foreach (var subId in dto.SelectedSubjectIDs)
                    {
                        var newLink = new GradeSubject
                        {
                            GradeId = dto.GradeID,
                            SubjectId = subId
                        };
                        await _gradeSubjectRepo.AddAsync(newLink);
                    }
                    await _gradeSubjectRepo.SaveChangesAsync();
                }

                await _gradeSubjectRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _gradeSubjectRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> SaveExamScheduleAsync(SaveExamScheduleDto dto)
        {

            if (dto.ScheduleImageFile == null || dto.ScheduleImageFile.Length == 0)
                throw new ArgumentException("ملف صورة جدول الامتحانات المرفوع فارغ أو تالف.");

            string fileExtension = Path.GetExtension(dto.ScheduleImageFile.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

           
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "exams");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string physicalFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
            {
                await dto.ScheduleImageFile.CopyToAsync(fileStream);
            }

            string relativeDatabasePath = $"uploads/exams/{uniqueFileName}";

            var allSchedules = await _examScheduleRepo.GetAllWithIncludeAndFilterAsync(
                es => es.GradeId == dto.GradeID && es.Semester == dto.Semester && es.AcademicYear == dto.AcademicYear
            );

            var existingSchedule = allSchedules.FirstOrDefault();

            if (existingSchedule != null)
            {
                if (!string.IsNullOrEmpty(existingSchedule.ImagePath))
                {
                    string oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSchedule.ImagePath);
                    if (File.Exists(oldPhysicalPath))
                    {
                        File.Delete(oldPhysicalPath);
                    }
                }

                existingSchedule.ImagePath = relativeDatabasePath;
                existingSchedule.UpdatedAt = DateTime.UtcNow;
                _examScheduleRepo.UpdateAsync(existingSchedule);
            }
            else
            {
                var newSchedule = new ExamSchedule
                {
                    GradeId = dto.GradeID,
                    Semester = dto.Semester,
                    ImagePath = relativeDatabasePath,
                    AcademicYear = dto.AcademicYear,
                    UpdatedAt = DateTime.UtcNow
                };
                await _examScheduleRepo.AddAsync(newSchedule);
            }

            await _examScheduleRepo.SaveChangesAsync();
            return true;
        }

        public async Task<SchoolAnnouncementResponseDto> CreateSchoolAnnouncementAsync(SchoolAnnouncementCreateDto dto, int senderPersonId = 0)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto), "بيانات الإعلان مطلوبة.");

            int validSenderId = senderPersonId;
            if (validSenderId <= 0)
            {
                var allPeople = await _personRepo.GetAllAsync();
                validSenderId = allPeople.FirstOrDefault()?.PersonId ?? 1;
            }
            else
            {
                var personExists = await _personRepo.GetByIdAsync(validSenderId);
                if (personExists == null)
                {
                    var allPeople = await _personRepo.GetAllAsync();
                    validSenderId = allPeople.FirstOrDefault()?.PersonId ?? 1;
                }
            }

            var announcement = new Announcement
            {
                Title = dto.Title.Trim(),
                AnnouncementBody = dto.Content.Trim(),
                IsGeneral = dto.IsGeneral,
                SenderPersonId = validSenderId,
                CreatedAt = DateTime.UtcNow
            };

            await _announcementRepo.AddAsync(announcement);
            await _announcementRepo.SaveChangesAsync();

            var sender = await _personRepo.GetByIdAsync(validSenderId);
            string senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Administration";

            return new SchoolAnnouncementResponseDto
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.AnnouncementBody,
                IsGeneral = announcement.IsGeneral,
                CreatedAt = announcement.CreatedAt ?? DateTime.UtcNow,
                SenderName = senderName
            };
        }

        public async Task<AdminFinanceDashboardDto> GetFinanceDashboardAsync()
        {
            var financeDashboard = new AdminFinanceDashboardDto();

            var allTeachers = await _teacherRepo.GetAllAsync();
            var allSupervisors = await _supervisorRepo.GetAllAsync();
            var allManagers = await _managerRepo.GetAllAsync();
            var allAccountants = await _accountantRepo.GetAllAsync();

            decimal totalTeachersSalary = allTeachers.Sum(t => (decimal)((t.WeeklyClasses ?? 0) * (t.SalaryPerClass ?? 0)));
            decimal totalSupervisorsSalary = allSupervisors.Sum(s => s.Salary ?? 0m);
            decimal totalManagersSalary = allManagers.Sum(m => m.Salary ?? 0m);
            decimal totalAccountantsSalary = allAccountants.Sum(a => a.Salary ?? 0m);

            decimal totalPayments = totalTeachersSalary + totalSupervisorsSalary + totalManagersSalary + totalAccountantsSalary;
            financeDashboard.TotalPayments = totalPayments;

            var allGrades = await _gradeRepo.GetAllAsync();
            var allClassPayments = await _classPaymentRepo.GetAllAsync();
            var allStudentRecords = await _studentRecordRepo.GetAllAsync();

            decimal totalReceivables = 0m;
            int totalStudents = 0;

            foreach (var grade in allGrades.OrderBy(g => g.GradeNumber))
            {
                int studentsInGrade = allStudentRecords.Count(sr => sr.GradeId == grade.GradeId);
                totalStudents += studentsInGrade;

                var classPayment = allClassPayments.FirstOrDefault(cp => cp.Class == (byte)grade.GradeNumber || cp.Class == (byte)grade.GradeId);
                decimal fee = classPayment?.FullAmount ?? 0m;

                if (fee == 0m && studentsInGrade > 0)
                {
                    var sampleRecord = allStudentRecords.FirstOrDefault(sr => sr.GradeId == grade.GradeId && sr.YearlyPayment != null && sr.YearlyPayment > 0);
                    if (sampleRecord != null)
                    {
                        fee = (decimal)sampleRecord.YearlyPayment;
                    }
                }

                decimal gradeTotalAmount = fee * studentsInGrade;
                totalReceivables += gradeTotalAmount;

                financeDashboard.TuitionFeesByGrade.Add(new GradeTuitionFeeGridItemDto
                {
                    GradeId = grade.GradeId,
                    GradeNumber = grade.GradeNumber,
                    GradeName = $"Grade {grade.GradeNumber}",
                    TuitionFee = fee,
                    StudentsCount = studentsInGrade,
                    TotalAmount = gradeTotalAmount
                });
            }

            financeDashboard.TotalReceivables = totalReceivables;
            financeDashboard.TotalTuitionReceivables = totalReceivables;
            financeDashboard.TotalStudentsCount = totalStudents;
            financeDashboard.NetBalance = totalReceivables - totalPayments;

            return financeDashboard;
        }

        public async Task<bool> UpdateGradeTuitionFeeAsync(UpdateGradeTuitionFeeDto dto)
        {
            if (dto == null) return false;

            var grade = await _gradeRepo.GetByIdAsync(dto.GradeId);
            if (grade == null)
            {
                var allGrades = await _gradeRepo.GetAllAsync();
                grade = allGrades.FirstOrDefault(g => g.GradeNumber == dto.GradeId);
                if (grade == null) return false;
            }

            byte classByte = (byte)grade.GradeNumber;

            var allClassPayments = await _classPaymentRepo.GetAllAsync();
            var existingClassPayment = allClassPayments.FirstOrDefault(cp => cp.Class == classByte || cp.Class == (byte)grade.GradeId);

            if (existingClassPayment != null)
            {
                existingClassPayment.FullAmount = dto.TuitionFee;
                existingClassPayment.Class = classByte;
                _classPaymentRepo.UpdateAsync(existingClassPayment);
            }
            else
            {
                var newClassPayment = new ClassPayment
                {
                    Class = classByte,
                    FullAmount = dto.TuitionFee
                };
                await _classPaymentRepo.AddAsync(newClassPayment);
            }
            await _classPaymentRepo.SaveChangesAsync();

            var studentRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(sr => sr.GradeId == grade.GradeId);
            foreach (var record in studentRecords)
            {
                record.YearlyPayment = dto.TuitionFee;
                _studentRecordRepo.UpdateAsync(record);
            }
            if (studentRecords.Any())
            {
                await _studentRecordRepo.SaveChangesAsync();
            }

            return true;
        }


        public async Task<string?> RegisterAccountantWorkflowAsync(CreateAccountantDto dto)
        {
            var allUsers = await _userRepo.GetAllAsync();

            bool isPhoneDuplicated = allUsers.Any(u => u.PhoneNumber.Trim() == dto.PhoneNumber.Trim());
            if (isPhoneDuplicated)
                throw new InvalidOperationException("رقم الهاتف هذا مسجل بالفعل لمستخدم آخر في النظام.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool isEmailDuplicated = allUsers.Any(u => u.Email != null && u.Email.Trim().ToLower() == dto.Email.Trim().ToLower());
                if (isEmailDuplicated)
                    throw new InvalidOperationException("البريد الإلكتروني هذا مستخدم بالفعل في حساب آخر.");
            }

            var transaction = await _classRoomRepo.BeginTransactionAsync();
            string generatedAccountNumber = string.Empty;

            try
            {
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); 
                var newUser = new User
                {
                    PersonId = newPerson.PersonId, 
                    UserRoleId = 6, 
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null,
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                var newAccountant = new Accountant
                {
                    PersonId = newPerson.PersonId, 
                    Salary = dto.Salary 
                };
                await _accountantRepo.AddAsync(newAccountant);
                await _accountantRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _classRoomRepo.RollbackTransactionAsync();
                throw new Exception($"فشلت عملية إدخال المحاسب في قاعدة البيانات: {ex.Message}");
            }

            try
            {
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    await _emailService.SendUserNumberAsync(dto.Email.Trim(), generatedAccountNumber);
                }
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"[Email Service Warning] Failed to deliver SMTP numbers: {emailEx.Message}");
            }

            return generatedAccountNumber;
        }



        public async Task<AdminAccountantsDashboardDto> GetAccountantsGridAsync(int page)
        {
            var dashboard = new AdminAccountantsDashboardDto();
            const int pageSize = 8; 
            var allAccountants = await _accountantRepo.GetAllWithIncludeAsync(a => a.Person);

            var allUsers = await _userRepo.GetAllAsync();

            var activeAccountants = allAccountants.Where(a => a.Person.IsActive).ToList();

            dashboard.TotalAccountantsCount = activeAccountants.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalAccountantsCount / pageSize);

            var paginatedAccountants = activeAccountants
                .OrderBy(a => a.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var accountant in paginatedAccountants)
            {
                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == accountant.PersonId);

                string cleanFullName = $"{accountant.Person.FirstName} {accountant.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Accountants.Add(new AdminAccountantGridItemDto
                {
                    AccountantID = accountant.AccountantId, 
                    FullName = cleanFullName,
                    Phone = matchedUser?.PhoneNumber ?? "No Phone Number",
                    Salary = accountant.Salary ?? 0, 
                    AccountNumber = matchedUser?.AccountNumber ?? "N/A"
                });
            }

            return dashboard;
        }

        public async Task<bool> UpdateTeacherWorkflowAsync(int teacherId, UpdateTeacherDto dto)
        {
            var allTeachers = await _teacherRepo.GetAllWithIncludeAsync(t => t.Person);
            var targetTeacher = allTeachers.FirstOrDefault(t => t.TeacherId == teacherId);
            if (targetTeacher == null || targetTeacher.Person == null)
                return false; 
            var allUsers = await _userRepo.GetAllAsync();

            bool isPhoneDuplicated = allUsers.Any(u => u.PhoneNumber.Trim() == dto.PhoneNumber.Trim() && u.PersonId != targetTeacher.PersonId);
            if (isPhoneDuplicated)
                throw new InvalidOperationException("رقم الهاتف هذا مسجل بالفعل لمستخدم آخر في النظام.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool isEmailDuplicated = allUsers.Any(u => u.Email != null && u.Email.Trim().ToLower() == dto.Email.Trim().ToLower() && u.PersonId != targetTeacher.PersonId);
                if (isEmailDuplicated)
                    throw new InvalidOperationException("البريد الإلكتروني هذا مستخدم بالفعل في حساب آخر.");
            }

            var userLog = allUsers.FirstOrDefault(u => u.PersonId == targetTeacher.PersonId);
            if (userLog == null)
                throw new InvalidOperationException("لم يتم العثور على حساب مستخدم (User) مرتبط بهذا الأستاذ.");

            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                targetTeacher.Person.FirstName = dto.FirstName.Trim();
                targetTeacher.Person.SecondName = dto.SecondName.Trim();
                targetTeacher.Person.LastName = dto.LastName.Trim();
                targetTeacher.Person.DateOfBirth = dto.DateOfBirth;
                targetTeacher.Person.Gender = dto.Gender;
                _personRepo.UpdateAsync(targetTeacher.Person);

                userLog.PhoneNumber = dto.PhoneNumber.Trim();
                userLog.Email = dto.Email?.Trim().ToLower();
                _userRepo.UpdateAsync(userLog);

                targetTeacher.WeeklyClasses = dto.WeeklyClasses;
                targetTeacher.SalaryPerClass = dto.SalaryPerClass;
                _teacherRepo.UpdateAsync(targetTeacher);

                await _teacherRepo.SaveChangesAsync();
                await _userRepo.SaveChangesAsync();
                await _personRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _classRoomRepo.RollbackTransactionAsync();
                throw new Exception($"فشلت عملية تحديث بيانات المعلم في قاعدة البيانات: {ex.Message}");
            }
        }

    }
}