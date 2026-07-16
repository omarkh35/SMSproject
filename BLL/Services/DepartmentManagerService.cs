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
        

        public async Task<StudentDirectoryDashboardDto> GetStudentDirectoryDashboardAsync(int managerPersonId, string? searchName, int page)
        {
            var dashboard = new StudentDirectoryDashboardDto();
            const int pageSize = 8;

            // 1. جلب الموجهين المرتبطين بمدير القسم الحالي عبر PersonID بالحروف الكبيرة
            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.DepartmentManager);
            var activeSupervisorIds = allSupervisors
                .Where(s => s.DepartmentManager.PersonId == managerPersonId)
                .Select(s => s.SupervisorId)
                .ToList();

            // 2. جلب الصفوف الخاضعة لإشراف هؤلاء الموجهين
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var managedClassRooms = allClassRooms
                .Where(cr => cr.SupervisorId != null && activeSupervisorIds.Contains(cr.SupervisorId.Value))
                .ToList();
            var managedClassRoomIds = managedClassRooms.Select(cr => cr.ClassRoomId).ToList();

            // 3. تحديد الطلاب الفعليين داخل هذه الغرف الصفية
            var allClassroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.ClassRoom);
            var managedClassroomStudents = allClassroomStudents
                .Where(cs => managedClassRoomIds.Contains(cs.ClassRoomId))
                .ToList();
            var managedStudentIds = managedClassroomStudents.Select(cs => cs.StudentId).Distinct().ToList();

            // 4. سحب سجلات الطلاب وتطبيق شرط البحث بالاسم (FirstName, SecondName, LastName)
            var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
            var filteredStudentRecords = allStudentRecords.Where(sr => managedStudentIds.Contains(sr.StudentId));

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredStudentRecords = filteredStudentRecords.Where(sr =>
                    sr.Student.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            var relevantStudentRecords = filteredStudentRecords.ToList();

            dashboard.TotalStudentsCount = relevantStudentRecords.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            // 5. حساب نسبة النجاح الأكاديمية (Success Rate) للفصل الأول
            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.ExamType);
            var sem1Exams = allMarks
                .Where(m => m.IsApproved && m.ExamType.Semester == 1 && managedStudentIds.Contains(m.StudentRecordId))
                .ToList();

            if (sem1Exams.Any())
            {
                int passingMarks = sem1Exams.Count(m => m.MarkValue >= (m.FullMark / 2));
                double percentage = ((double)passingMarks / sem1Exams.Count) * 100;
                dashboard.PassRate = $"{percentage:F0}%";
            }
            else
            {
                dashboard.PassRate = "N/A";
            }

            // 6. تطبيق منطق الصفحات للجدول
            var paginatedRecords = relevantStudentRecords
                .OrderBy(sr => sr.Student.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var allStudentParents = await _studentParentRepo.GetAllWithIncludeAsync(sp => sp.Parent, sp => sp.Parent.Person, sp => sp.Parent.Person.Users);

            // 7. صياغة المخرجات النهائية وضمان بقاء الـ Section رقماً
            foreach (var record in paginatedRecords)
            {
                string gradeDisplay = "-";
                int sectionDisplay = 0; // القيمة الافتراضية كرقم لقاعدة البيانات

                var assignedClassLink = managedClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
                if (assignedClassLink != null)
                {
                    var matchedRoom = managedClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        gradeDisplay = $"{matchedRoom.Grade.GradeNumber}th";
                        sectionDisplay = matchedRoom.Section; // إرجاع القيمة الرقمية المباشرة (1, 2, 3...) دون أي تغيير
                    }
                }

                var parentLink = allStudentParents.FirstOrDefault(sp => sp.StudentId == record.StudentId);
                var parentUserAccount = parentLink?.Parent?.Person?.Users?.FirstOrDefault();
                string parentPhone = parentUserAccount?.PhoneNumber ?? "No Number";

                string cleanFullName = $"{record.Student.Person.FirstName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Students.Add(new StudentGridItemDto
                {
                    StudentID = record.StudentId,
                    StudentName = cleanFullName,
                    Grade = gradeDisplay,
                    Section = sectionDisplay, // رقم نقي تماماً
                    Phone = parentPhone
                });
            }

            return dashboard;
        }

        public async Task<SupervisorsDashboardDto> GetSupervisorsManagementDashboardAsync(int managerPersonId)
        {
            var dashboard = new SupervisorsDashboardDto();

            // 1. Fetch all classrooms globally to calculate cross-sectional card summaries
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync();

            // Open Sections rule: Count classrooms that have no supervisor assigned
            dashboard.OpenSections = allClassRooms.Count(cr => cr.SupervisorId == null);

            // 2. Fetch all supervisors under this specific Department Manager's oversight line
            var baseSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(
                s => s.Person,
                s => s.DepartmentManager
            );

            var managedSupervisors = baseSupervisors
                .Where(s => s.DepartmentManager.PersonId == managerPersonId)
                .ToList();

            dashboard.TotalSupervisors = managedSupervisors.Count;

            // 3. Gather user security contact channels to resolve direct telephone links
            var allUsers = await _userRepo.GetAllWithIncludeAsync();

            // 4. Hydrate row array metrics
            foreach (var sup in managedSupervisors)
            {
                // Sections rule: Calculate count of active classrooms assigned to this distinct supervisor ID
                int supervisedCount = allClassRooms.Count(cr => cr.SupervisorId == sup.SupervisorId);

                var relatedUser = allUsers.FirstOrDefault(u => u.PersonId == sup.PersonId);
                string phoneContact = relatedUser?.PhoneNumber ?? "No Number";

                string calculatedStatus = sup.Person.IsActive ? "Active" : "Inactive";
                string combinedFullName = $"{sup.Person.FirstName} {sup.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Supervisors.Add(new SupervisorGridItemDto
                {
                    SupervisorID = sup.SupervisorId,
                    FullName = combinedFullName,
                    Phone = phoneContact,
                    Status = calculatedStatus,
                    SectionsCount = supervisedCount // Outputs flat integer amount directly
                });
            }

            // Assigned Sections card metric: Accumulate sum of all sections handled by active supervisors combined
            dashboard.AssignedSections = dashboard.Supervisors.Sum(s => s.SectionsCount);

            return dashboard;
        }

        public async Task<TeachersDashboardDto> GetTeachersManagementDashboardAsync()
        {
            var dashboard = new TeachersDashboardDto();

            // 1. جلب جميع المعلمين النشطين في النظام مع معلوماتهم الشخصية
            var allTeachers = await _teacherRepo.GetAllWithIncludeAsync(t => t.Person);
            var activeTeachers = allTeachers.Where(t => t.Person.IsActive).ToList();

            dashboard.TotalTeachers = activeTeachers.Count;

            // 2. جلب حسابات المستخدمين لاستخراج أرقام الهواتف
            var allUsers = await _userRepo.GetAllWithIncludeAsync();

            // 3. بناء الأسطر لجدول المعلمين
            foreach (var teacher in activeTeachers)
            {
                var relatedUser = allUsers.FirstOrDefault(u => u.PersonId == teacher.PersonId);
                string phoneContact = relatedUser?.PhoneNumber ?? "No Number";

                string combinedFullName = $"{teacher.Person.FirstName} {teacher.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Teachers.Add(new TeacherGridItemDto
                {
                    TeacherID = teacher.TeacherId,
                    FullName = combinedFullName,
                    Phone = phoneContact,
                    Lessons = teacher.WeeklyClasses ?? 0 // عرض الحصص الأسبوعية مباشرة كرقم
                });
            }

            return dashboard;
        }

    }
}
