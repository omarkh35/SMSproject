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
        IBaseRepositories<Accountant> accountantRepo)
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
        }

        // =========================================================================
        // 1. إدارة المواد التعليمية (Subjects CRUD) - مصححة لمطابقة المسميات
        // =========================================================================
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
                Id = subject.SubjectId, // استخدام الحروف الكبيرة الكفيلة بمطابقة الكود
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

        // =========================================================================
        // 2. واجهة إدارة مدراء الأقسام (Department Managers) - المحدثة والمطورة بالكامل
        // =========================================================================
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
                    AccountNumber = associatedUser?.AccountNumber ?? "N/A" // إرجاع رقم الحساب للواجهة
                };
            }).ToList();
        }

        public async Task<StaffDto?> AddDepartmentManagerAsync(DepartmentManagerCreateDto dto)
        {
            // فتح ترانزكشن لحماية دورة إنشاء الموظف الكاملة (Person -> User -> Financial Record)
            var transaction = await _classRoomRepo.BeginTransactionAsync();
            try
            {
                // أ. توليد رقم الحساب الفريد من السيكوينس المعتمد لديك لإعداده للـ SMS
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                // ب. إنشاء السجل الشخصي الأساسي للموظف الجديد
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
                await _personRepo.SaveChangesAsync(); // توليد الـ PersonID

                // ج. إنشاء حساب المستخدم المقفل - معرّف دور مدير القسم هو 1 في نظام الأدوار لديك
                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 1, // Department Manager Role Code
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null, // يُترك فارغاً للتفعيل الذاتي لاحقاً عند أول تسجيل دخول
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                // د. إنشاء السجل المالي والوظيفي الخاص بمدير القسم
                var manager = new DepartmentManager
                {
                    PersonId = newPerson.PersonId,
                    Salary = dto.Salary
                };
                await _managerRepo.AddAsync(manager);
                await _managerRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();

                return new StaffDto
                {
                    Id = manager.DepartmentManagerId,
                    PersonId = manager.PersonId,
                    FullName = $"{newPerson.FirstName} {newPerson.LastName}".Trim(),
                    Salary = manager.Salary,
                    Role = "DEPARTMENT MANAGER",
                    AccountNumber = generatedAccountNumber // إرجاع رقم الحساب المولد لعرضه فوراً وإرساله كـ SMS
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

        // =========================================================================
        // 3. واجهة إدارة الموجهين (Supervisors) - تم تصفيتها لحل مشكلة الـ PersonId
        // =========================================================================
        public async Task<IEnumerable<StaffDto>> GetAllSupervisorsAsync()
        {
            // جلب البيانات بشكل مسطح ومنفصل لحماية الاستعلام من الـ Shadow Property Trap العكسي
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
                // أ. توليد رقم الحساب الفريد من السيكوينس المعتمد في السكريبت باستخدام دالتك الصحيحة تماماً
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                // ب. إنشاء الشخص الجديد (مطابقة لأسماء خصائص كلاس الـ Person لديك)
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
                await _personRepo.SaveChangesAsync(); // هنا يتولد الـ newPerson.PersonId تلقائياً

                // ج. إنشاء المستخدم (مطابقة لأسماء خصائص كلاس الـ User لديك) - معرّف دور الموجه هو 4
                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 4, // Supervisor Role Code
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null, // متروك للتفعيل الذاتي لاحقاً عند أول تسجيل دخول
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                // د. إنشاء الموجه وربطه الإلزامي بمدير القسم المحدد (مطابقة لأسماء خصائص كلاس الـ Supervisor لديك)
                var supervisor = new Supervisor
                {
                    PersonId = newPerson.PersonId,
                    Salary = dto.Salary,
                    DepartmentManagerId = dto.DepartmentManagerId
                };
                await _supervisorRepo.AddAsync(supervisor);
                await _supervisorRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();

                return new StaffDto
                {
                    Id = supervisor.SupervisorId,
                    PersonId = supervisor.PersonId,
                    FullName = $"{newPerson.FirstName} {newPerson.LastName}".Trim(),
                    Salary = supervisor.Salary,
                    Role = "SUPERVISOR",
                    AccountNumber = generatedAccountNumber // جاهز للإرسال الفوري للـ SMS مستقبلاً
                };
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return null;
            }
        }

        // تصحيح: تغيير نوع الإرجاع إلى Task<bool> ليتوافق مع الـ return true/false في كودك
        public async Task<bool> UpdateSupervisorAsync(int id, StaffUpdateDto dto)
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if (supervisor == null) return false;

            supervisor.Salary = dto.Salary;
            _supervisorRepo.UpdateAsync(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            return true;
        }

        // تصحيح: تغيير نوع الإرجاع إلى Task<bool> ليتوافق مع الـ return true/false في كودك
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

            // =========================================================================
            // جلب البيانات المسطحة لضمان السرعة القصوى (High Performance Pull)
            // =========================================================================
            var allGrades = await _gradeRepo.GetAllAsync();
            var allStudentRecords = await _studentRecordRepo.GetAllAsync();
            var allTeachers = await _teacherRepo.GetAllAsync();
            var allManagers = await _managerRepo.GetAllAsync();
            var allSupervisors = await _supervisorRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllAsync();
            var allClassTeachers = await _classTeacherRepo.GetAllAsync();
            var allGradeSubjects = await _gradeSubjectRepo.GetAllAsync();
            var allMarks = await _markRepo.GetAllAsync();
            var allAnnouncements = await _announcementRepo.GetAllAsync(); // جلب الإعلانات

            // 1. حساب بطاقات الملخص العلوية (Top Cards)
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

            // =========================================================================
            // 💡 القسم الجديد: صياغة وبناء شريط الإعلانات الحركي (Announcements Feed)
            // =========================================================================
            // جلب أحدث الإعلانات المسجلة وترتيبها من الأحدث للأقدم لتعرض أولاً في التمرير الأفقي
            var latestAnnouncements = allAnnouncements
                .OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue)
                .Take(10) // نأخذ أحدث 10 إعلانات فقط للحفاظ على رشاقة واجهة المستخدم
                .ToList();

            foreach (var announce in latestAnnouncements)
            {
                // اختصار نص الإعلان الطويل ليناسب مساحة الكارت الصغير في الواجهة المرئية
                string textSummary = announce.AnnouncementBody.Length > 40
                    ? announce.AnnouncementBody.Substring(0, 37) + "..."
                    : announce.AnnouncementBody;

                dashboard.Announcements.Add(new DashboardAnnouncementItemDto
                {
                    AnnouncementID = announce.AnnouncementId,
                    Title = announce.Title,
                    BodySummary = textSummary,
                    // ترجمة القيمة البولينية (IsGeneral) لتعرض التوصيف الفئوي بدقة ("All" أو "Parents")
                    TargetAudience = announce.IsGeneral ? "All" : "Parents",
                    // تنسيق التاريخ ليطابق الشكل المعروض في واجهاتك بالظبط مثل "Jul 15, 2026"
                    CreatedDateStr = announce.CreatedAt.HasValue
                        ? announce.CreatedAt.Value.ToString("MMM dd, yyyy")
                        : "Recent"
                });
            }

            var sortedGrades = allGrades.OrderBy(g => g.GradeNumber).ToList();

            // 2. بناء جدول الطلاب والشعب لكل صف (Students per Grade)
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

            // 3. بناء جدول المعلمين والمواد لكل صف (Teachers per Grade)
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
            const int pageSize = 8; // تقسيم العرض لصفحات كل صفحة مؤلفة من 8 أساتذة بناءً على طلبك

            // 1. جلب البيانات المسطحة لضمان السرعة ومنع تخمين الحقول الوهمية
            var allTeachers = await _teacherRepo.GetAllWithIncludeAsync(t => t.Person);
            var allUsers = await _userRepo.GetAllAsync();
            var allClassTeachers = await _classTeacherRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);

            // 2. تطبيق البحث عن اسم الأستاذ (بناءً على الاسم الأول، الأب، أو العائلة)
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

            // 3. احتساب القياسات العامة والصفحات (Pagination Metrics)
            dashboard.TotalTeachersCount = matchingTeachersList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalTeachersCount / pageSize);

            // 4. جلب الشريحة الخاصة بالصفحة الحالية المطلوبة
            var paginatedTeachers = matchingTeachersList
                .OrderBy(t => t.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5. معالجة وبناء سجلات شبكة العرض
            foreach (var teacher in paginatedTeachers)
            {
                // أ. حساب الراتب (ضرب الساعات الأسبوعية في أجرة الحصة المعتمد بجدولك)
                decimal calculatedSalary = (teacher.WeeklyClasses ?? 0) * (decimal)(teacher.SalaryPerClass ?? 0);

                // ب. استخراج الهاتف من جدول المستخدمين بأمان عبر الذاكرة
                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == teacher.PersonId);
                string phoneContact = matchedUser?.PhoneNumber ?? "No Phone Locked";

                // ج. استخراج الصفوف الفريدة التي يدرسها الأستاذ ديناميكياً وعرضها كأرقام نقية
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

                // تحويل مصفوفة أرقام الصفوف إلى نص منسق يفصل بينها فاصلة (مثل "9, 10") لتطابق الواجهة
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
            const int pageSize = 8; // عرض 8 موجهين لكل صفحة

            // 1. جلب البيانات مسطحة لضمان السرعة القصوى وعزل العلاقات الدائرية
            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.Person);
            var allUsers = await _userRepo.GetAllAsync();
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);

            // 2. تطبيق فلترة البحث باسم الموجه (الأول، الأب، العائلة)
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

            // 3. حساب إجماليات الصفحات
            dashboard.TotalSupervisorsCount = matchingSupervisorsList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalSupervisorsCount / pageSize);

            var paginatedSupervisors = matchingSupervisorsList
                .OrderBy(s => s.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. بناء سطر كل موجه ومعالجة معادلة الشعب المخصصة
            foreach (var supervisor in paginatedSupervisors)
            {
                // أ. استخراج الهاتف بأمان من الذاكرة
                var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == supervisor.PersonId);
                string phoneContact = matchedUser?.PhoneNumber ?? "No Phone Locked";

                // ب. جلب الغرف الصفية المسجلة تحت إشراف هذا الموجه بالذات
                var supervisedRooms = allClassRooms
                    .Where(cr => cr.SupervisorId == supervisor.SupervisorId)
                    .ToList();

                string formattedSectionsStr = "None";

                if (supervisedRooms.Any())
                {
                    // ج. تنفيذ المعادلة: تجميع الغرف الصفية بناءً على رقم الصف (GradeNumber)
                    var groupedByGrade = supervisedRooms
                        .GroupBy(cr => cr.Grade.GradeNumber)
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            // ترتيب أرقام الشعب تصاعدياً داخل الصف (مثل: 2, 3)
                            var sectionNumbers = g.Select(cr => cr.Section).OrderBy(s => s).ToList();
                            string sectionsInsideStr = string.Join(",", sectionNumbers);

                            // دمجهم بالصيغة المطلوبة: "5(2,3)"
                            return $"{g.Key}({sectionsInsideStr})";
                        });

                    // د. دمج الصفوف المختلفة بفواصل عادية ليصبح النص النهائي: "5(2,3), 4(7)"
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
                    Sections = formattedSectionsStr, // النص المنسق والمحسوب رياضياً بناءً على طلبك
                    Salary = supervisor.Salary ?? 0
                });
            }

            return dashboard;
        }




        public async Task<AdminManagersDashboardDto> GetDepartmentManagersGridAsync(string? searchName, int page)
        {
            var dashboard = new AdminManagersDashboardDto();
            const int pageSize = 8; // تقسيم عرض مدراء الأقسام لـ 8 أسطر في الصفحة الواحدة

            // 1. جلب البيانات مسطحة بشكل منفصل لحماية المزامنة البرمجية
            var allManagers = await _managerRepo.GetAllWithIncludeAsync(m => m.Person);
            var allUsers = await _userRepo.GetAllAsync();

            // 2. تطبيق منطق البحث الفوري باسم مدير القسم (الأول، الأب، العائلة)
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

            // 3. حساب مقاييس ترقيم الصفحات (Pagination)
            dashboard.TotalManagersCount = matchingList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalManagersCount / pageSize);

            var paginatedData = matchingList
                .OrderBy(m => m.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. رسم خلايا الجدول ومطابقة الأرقام المالية بدقة
            foreach (var manager in paginatedData)
            {
                // استخراج الهاتف بأمان وسرعة فائقة من الذاكرة عبر الـ PersonId الموثق لديك
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
                    Salary = manager.Salary ?? 0 // يطابق حقل الـ money المعتمد
                });
            }

            return dashboard;
        }


        public async Task<AdminStudentsDashboardDto> GetStudentsManagementGridAsync(string? searchName, int? gradeId, int? sectionNumber, int page)
        {
            var dashboard = new AdminStudentsDashboardDto();
            const int pageSize = 8; // تقسيم عرض الطلاب لـ 8 أسطر في الصفحة الواحدة

            // 1. جلب قوائم الصفوف (Grades) لتغذية القائمة المنسدلة في الواجهة
            var allGrades = await _gradeRepo.GetAllAsync();
            dashboard.AvailableGrades = allGrades.OrderBy(g => g.GradeNumber).Select(g => new GradeDropdownItemDto
            {
                GradeID = g.GradeId,
                GradeDisplayName = $"Grade {g.GradeNumber}"
            }).ToList();

            // 2. جلب البيانات مسطحة بشكل منفصل لحماية المزامنة وسرعة الاستعلام
            var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var allClassroomStudents = await _classStudentRepo.GetAllAsync();

            dashboard.AvailableSections = allClassRooms
    .Select(cr => (int)cr.Section)
    .Distinct()
    .OrderBy(s => s)
    .ToList();
            // 3. بناء مصفوفة الطلاب المبدئية لحساب شروط الفلترة بمرونة عالية
            var fullStudentsList = new List<AdminStudentGridItemDto>();

            foreach (var record in allStudentRecords)
            {
                string gradeDisplayStr = "Not Assigned";
                int sectionDisplayNum = 0;
                int currentClassRoomId = 0;

                // استخراج رابط الصف والغرفة الصفية للطالب من جدول ClassroomStudent
                var assignedClassLink = allClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
                if (assignedClassLink != null)
                {
                    var matchedRoom = allClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        gradeDisplayStr = $"Grade {matchedRoom.Grade.GradeNumber}";
                        sectionDisplayNum = matchedRoom.Section; // رقم نقي مباشر
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

            // 4. تطبيق شروط الفلترة المتقاطعة (اسم، أو صف لوحده، أو صف وشعبة معاً)
            var filteredQuery = fullStudentsList.AsEnumerable();

            // أ. الفلترة بحسب الاسم (البحث الجزئي)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredQuery = filteredQuery.Where(s => s.StudentName.ToLower().Contains(cleanSearch));
            }

            // ب. الفلترة بحسب الـ Grade لوحدها
            if (gradeId.HasValue)
            {
                var matchedGrade = allGrades.FirstOrDefault(g => g.GradeId == gradeId.Value);
                if (matchedGrade != null)
                {
                    string targetGradeStr = $"Grade {matchedGrade.GradeNumber}";
                    filteredQuery = filteredQuery.Where(s => s.Grade == targetGradeStr);
                }
            }

            // ج. الفلترة بحسب الـ Grade والـ Section سوياً
            // (الشرط يتحقق من تمرير رقم الشعبة مع التأكيد على وجود معرف الصف أيضاً لضمان دقة الربط المتقاطع)
            if (gradeId.HasValue && sectionNumber.HasValue)
            {
                filteredQuery = filteredQuery.Where(s => s.Section == sectionNumber.Value);
            }

            var finalizedFilteredList = filteredQuery.ToList();

            // 5. حساب مقاييس ترقيم الصفحات (Pagination)
            dashboard.TotalStudentsCount = finalizedFilteredList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            // 6. تقسيم النتائج وإرسال الشريحة المطلوبة للواجهة
            dashboard.Students = finalizedFilteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return dashboard;
        }


        public async Task<GradeConfigViewDto> GetGradeConfigurationAsync(int gradeId)
        {
            var config = new GradeConfigViewDto { GradeID = gradeId };

            // جلب كل المواد المتوفرة في المدرسة، وجلب روابط الفرز الحالية لهذا الصف
            var allSubjects = await _subjectRepo.GetAllAsync();
            var currentLinks = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(gs => gs.GradeId == gradeId);

            // مصفوفة كلمات مرجعية لفرز نوع المادة تلقائياً بالذاكرة دون العبث بجدول المواد الفعلي
            
            foreach (var sub in allSubjects)
            {
                // اكتشاف النوع: إذا كان الاسم يحتوي على كلمة علمية يوضع له وسم Scientific وإلا Literary
                string upperName = sub.SubjectName.ToUpper();
               
                // فحص هل المادة مربوطة بالصف حالياً في الداتابيز لتظهر كـ Checked في الموبايل أو الويب
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

        // === 2. تزامنية حفظ التعديلات للمواد المختارة (Sync Matrix) ===
        public async Task<bool> SaveGradeSubjectsConfigurationAsync(SaveGradeSubjectsDto dto)
        {
            // فتح ترانزكشن لحفظ عمليات الحذف والإدخال المجمعة بأمان
            var transaction = await _gradeSubjectRepo.BeginTransactionAsync();
            try
            {
                // أ. مسح كل الروابط القديمة المخزنة لهذا الصف في جدول GradeSubject للبدء بصفحة بيضاء
                var existingLinks = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(gs => gs.GradeId == dto.GradeID);
                foreach (var link in existingLinks)
                {
                    _gradeSubjectRepo.Delete(link);
                }
                await _gradeSubjectRepo.SaveChangesAsync();

                // ب. إعادة حقن المواد الجديدة التي قام المدير باختيارها ووضع علامة صح عليها
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

        // === 3. الـ API الثاني: حفظ أو تحديث جدول الامتحانات (Upsert Exam Schedule) ===
        public async Task<bool> SaveExamScheduleAsync(SaveExamScheduleDto dto)
        {
            // فحص هل يوجد جدول امتحانات مضاف مسبقاً لنفس الصف والفصل والسنة لتحديثه بدلاً من تكرار السجلات
            var allSchedules = await _examScheduleRepo.GetAllWithIncludeAndFilterAsync(
                es => es.GradeId == dto.GradeID && es.Semester == dto.Semester && es.AcademicYear == dto.AcademicYear
            );

            var existingSchedule = allSchedules.FirstOrDefault();

            if (existingSchedule != null)
            {
                // عملية التحديث (UPDATE Image)
                existingSchedule.ImagePath = dto.ImagePath.Trim();
                existingSchedule.UpdatedAt = DateTime.UtcNow;
                _examScheduleRepo.UpdateAsync(existingSchedule);
            }
            else
            {
                // عملية الإدخال الجديد (INSERT)
                var newSchedule = new ExamSchedule
                {
                    GradeId = dto.GradeID,
                    Semester = dto.Semester,
                    ImagePath = dto.ImagePath.Trim(),
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

            // التحقق من هوية المرسل: إذا كان 0 أو غير موجود في جدول People، يتم إسناده لحساب إداري صالح
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

        // =========================================================================
        // 5. واجهة المالية والرواتب والأقساط (School Finance Dashboard)
        // =========================================================================
        public async Task<AdminFinanceDashboardDto> GetFinanceDashboardAsync()
        {
            var financeDashboard = new AdminFinanceDashboardDto();

            // 1. حساب إجمالي الرواتب المدفوعة لكافة طاقم المدرسة (TOTAL PAYMENTS - Salaries paid to staff)
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

            // 2. جلب الصفوف وأسعار الأقساط والطلاب المسجلين لكل صف (Tuition Fees by Grade)
            var allGrades = await _gradeRepo.GetAllAsync();
            var allClassPayments = await _classPaymentRepo.GetAllAsync();
            var allStudentRecords = await _studentRecordRepo.GetAllAsync();

            decimal totalReceivables = 0m;
            int totalStudents = 0;

            foreach (var grade in allGrades.OrderBy(g => g.GradeNumber))
            {
                // حساب عدد الطلاب المسجلين في هذا الصف
                int studentsInGrade = allStudentRecords.Count(sr => sr.GradeId == grade.GradeId);
                totalStudents += studentsInGrade;

                // جلب رسم القسط لهذا الصف من جدول ClassPayment
                var classPayment = allClassPayments.FirstOrDefault(cp => cp.Class == (byte)grade.GradeNumber || cp.Class == (byte)grade.GradeId);
                decimal fee = classPayment?.FullAmount ?? 0m;

                // في حال عدم وجود تسجيل سابق في جدول ClassPayment، نقرأ من القسط السنوي المسجل للطلاب
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

        // =========================================================================
        // 6. تعديل قسط صف معين وتحديث رسوم الطلاب (Edit Tuition Fee by Grade)
        // =========================================================================
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

            // 1. تحديث أو إنشاء سجل في جدول ClassPayment
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

            // 2. مزامنة القسط الدراسي مع سجلات طلاب هذا الصف في StudentRecords
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

    }
}