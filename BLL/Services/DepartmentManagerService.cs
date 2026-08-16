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
        IBaseRepositories<DepartmentManager> _managerRepo;
        private readonly IBaseRepositories<ChatRoom> _chatRoomRepo;
        private readonly IBaseRepositories<Parent> _parentRepo;

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
            IBaseRepositories<StudentParent> studentParentRepo,
        IBaseRepositories<DepartmentManager> managerRepo,
        IBaseRepositories<ChatRoom> chatRoomRepo,
            IBaseRepositories<Parent> parentRepo

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
            _managerRepo = managerRepo;
            _chatRoomRepo = chatRoomRepo;
            _parentRepo = parentRepo;

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

            if (newClass.SupervisorId.HasValue)
            {
                await AutoProvisionChatRoomsForClassRoomAsync(newClass.ClassRoomId, newClass.SupervisorId.Value);
            }

            return new ClassRoomDto
            {
                Id = newClass.ClassRoomId,
                GradeId = newClass.GradeId,
                Section = newClass.Section,
                StartYear = newClass.StartYear,
                SupervisorId = newClass.SupervisorId
            };
        }

        public async Task<bool> UpdateClassRoomAsync(int id, ClassRoomUpdateDto dto)
        {
            var existing = await _classRoomRepo.GetByIdAsync(id);
            if (existing == null) return false;

            int? oldSupervisorId = existing.SupervisorId;
            existing.StartYear = dto.StartYear ?? existing.StartYear;
            if (dto.Section.HasValue) existing.Section = dto.Section.Value;
            if (dto.SupervisorId.HasValue) existing.SupervisorId = dto.SupervisorId.Value;

            _classRoomRepo.UpdateAsync(existing);
            await _classRoomRepo.SaveChangesAsync();

            if (existing.SupervisorId.HasValue && existing.SupervisorId != oldSupervisorId)
            {
                await AutoProvisionChatRoomsForClassRoomAsync(existing.ClassRoomId, existing.SupervisorId.Value);
            }

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

            try
            {
                var classRoom = await _classRoomRepo.GetByIdAsync(dto.ClassRoomId);
                if (classRoom != null && classRoom.SupervisorId.HasValue)
                {
                    var supervisor = await _supervisorRepo.GetByIdAsync(classRoom.SupervisorId.Value);
                    if (supervisor != null)
                    {
                        var studentParents = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                            sp => sp.StudentId == dto.StudentId,
                            sp => sp.Parent
                        );

                        foreach (var sp in studentParents)
                        {
                            int parentPersonId = sp.Parent != null ? sp.Parent.PersonId : 0;
                            if (parentPersonId <= 0 && sp.ParentID.HasValue)
                            {
                                var p = await _parentRepo.GetByIdAsync(sp.ParentID.Value);
                                if (p != null) parentPersonId = p.PersonId;
                            }

                            if (parentPersonId > 0)
                            {
                                var existingRooms = await _chatRoomRepo.GetAllWithIncludeAndFilterAsync(
                                    cr => cr.StudentFocusId == dto.StudentId &&
                                          cr.SupervisorPersonId == supervisor.PersonId &&
                                          cr.ParentPersonId == parentPersonId
                                );

                                var room = existingRooms.FirstOrDefault();
                                if (room == null)
                                {
                                    var newRoom = new ChatRoom
                                    {
                                        StudentFocusId = dto.StudentId,
                                        SupervisorPersonId = supervisor.PersonId,
                                        ParentPersonId = parentPersonId,
                                        CreatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    await _chatRoomRepo.AddAsync(newRoom);
                                    await _chatRoomRepo.SaveChangesAsync();
                                }
                                else if (!room.IsActive)
                                {
                                    room.IsActive = true;
                                    _chatRoomRepo.UpdateAsync(room);
                                    await _chatRoomRepo.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Suppress exception to keep assignment successful
            }


            return true;
        }


        private async Task AutoProvisionChatRoomsForClassRoomAsync(int classRoomId, int supervisorId)
        {
            try
            {
                var supervisor = await _supervisorRepo.GetByIdAsync(supervisorId);
                if (supervisor == null) return;

                var classStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                    cs => cs.ClassRoomId == classRoomId
                );

                var studentIds = classStudents.Select(cs => cs.StudentId).Distinct().ToList();
                if (!studentIds.Any()) return;

                var studentParents = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                    sp => studentIds.Contains(sp.StudentId),
                    sp => sp.Parent
                );

                foreach (var sp in studentParents)
                {
                    int parentPersonId = sp.Parent != null ? sp.Parent.PersonId : 0;
                    if (parentPersonId <= 0 && sp.ParentID.HasValue)
                    {
                        var p = await _parentRepo.GetByIdAsync(sp.ParentID.Value);
                        if (p != null) parentPersonId = p.PersonId;
                    }

                    if (parentPersonId > 0)
                    {
                        var existingRooms = await _chatRoomRepo.GetAllWithIncludeAndFilterAsync(
                            cr => cr.StudentFocusId == sp.StudentId &&
                                  cr.SupervisorPersonId == supervisor.PersonId &&
                                  cr.ParentPersonId == parentPersonId
                        );

                        var room = existingRooms.FirstOrDefault();
                        if (room == null)
                        {
                            var newRoom = new ChatRoom
                            {
                                StudentFocusId = sp.StudentId,
                                SupervisorPersonId = supervisor.PersonId,
                                ParentPersonId = parentPersonId,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };
                            await _chatRoomRepo.AddAsync(newRoom);
                            await _chatRoomRepo.SaveChangesAsync();
                        }
                        else if (!room.IsActive)
                        {
                            room.IsActive = true;
                            _chatRoomRepo.UpdateAsync(room);
                            await _chatRoomRepo.SaveChangesAsync();
                        }
                    }
                }
            }
            catch
            {
                // Suppress exception
            }
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


        //public async Task<StudentDirectoryDashboardDto> GetStudentDirectoryDashboardAsync(int managerPersonId, string? searchName, int page)
        //{
        //    var dashboard = new StudentDirectoryDashboardDto();
        //    const int pageSize = 8;

        //    // 1. جلب الموجهين المرتبطين بمدير القسم الحالي عبر PersonID بالحروف الكبيرة
        //    var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.DepartmentManager);
        //    var activeSupervisorIds = allSupervisors
        //        .Where(s => s.DepartmentManager.PersonId == managerPersonId)
        //        .Select(s => s.SupervisorId)
        //        .ToList();

        //    // 2. جلب الصفوف الخاضعة لإشراف هؤلاء الموجهين
        //    var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
        //    var managedClassRooms = allClassRooms
        //        .Where(cr => cr.SupervisorId != null && activeSupervisorIds.Contains(cr.SupervisorId.Value))
        //        .ToList();
        //    var managedClassRoomIds = managedClassRooms.Select(cr => cr.ClassRoomId).ToList();

        //    // 3. تحديد الطلاب الفعليين داخل هذه الغرف الصفية
        //    var allClassroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.ClassRoom);
        //    var managedClassroomStudents = allClassroomStudents
        //        .Where(cs => managedClassRoomIds.Contains(cs.ClassRoomId))
        //        .ToList();
        //    var managedStudentIds = managedClassroomStudents.Select(cs => cs.StudentId).Distinct().ToList();

        //    // 4. سحب سجلات الطلاب وتطبيق شرط البحث بالاسم (FirstName, SecondName, LastName)
        //    var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
        //    var filteredStudentRecords = allStudentRecords.Where(sr => managedStudentIds.Contains(sr.StudentId));

        //    if (!string.IsNullOrWhiteSpace(searchName))
        //    {
        //        string cleanSearch = searchName.Trim().ToLower();
        //        filteredStudentRecords = filteredStudentRecords.Where(sr =>
        //            sr.Student.Person.FirstName.ToLower().Contains(cleanSearch) ||
        //            sr.Student.Person.SecondName.ToLower().Contains(cleanSearch) ||
        //            sr.Student.Person.LastName.ToLower().Contains(cleanSearch)
        //        );
        //    }

        //    var relevantStudentRecords = filteredStudentRecords.ToList();

        //    dashboard.TotalStudentsCount = relevantStudentRecords.Count;
        //    dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

        //    // 5. حساب نسبة النجاح الأكاديمية (Success Rate) للفصل الأول
        //    var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.ExamType);
        //    var sem1Exams = allMarks
        //        .Where(m => m.IsApproved && m.ExamType.Semester == 1 && managedStudentIds.Contains(m.StudentRecordId))
        //        .ToList();

        //    if (sem1Exams.Any())
        //    {
        //        int passingMarks = sem1Exams.Count(m => m.MarkValue >= (m.FullMark / 2));
        //        double percentage = ((double)passingMarks / sem1Exams.Count) * 100;
        //        dashboard.PassRate = $"{percentage:F0}%";
        //    }
        //    else
        //    {
        //        dashboard.PassRate = "N/A";
        //    }

        //    // 6. تطبيق منطق الصفحات للجدول
        //    var paginatedRecords = relevantStudentRecords
        //        .OrderBy(sr => sr.Student.Person.FirstName)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList();

        //    var allStudentParents = await _studentParentRepo.GetAllWithIncludeAsync(sp => sp.Parent, sp => sp.Parent.Person, sp => sp.Parent.Person.Users);

        //    // 7. صياغة المخرجات النهائية وضمان بقاء الـ Section رقماً
        //    foreach (var record in paginatedRecords)
        //    {
        //        string gradeDisplay = "-";
        //        int sectionDisplay = 0; // القيمة الافتراضية كرقم لقاعدة البيانات

        //        var assignedClassLink = managedClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
        //        if (assignedClassLink != null)
        //        {
        //            var matchedRoom = managedClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
        //            if (matchedRoom != null)
        //            {
        //                gradeDisplay = $"{matchedRoom.Grade.GradeNumber}th";
        //                sectionDisplay = matchedRoom.Section; // إرجاع القيمة الرقمية المباشرة (1, 2, 3...) دون أي تغيير
        //            }
        //        }

        //        var parentLink = allStudentParents.FirstOrDefault(sp => sp.StudentId == record.StudentId);
        //        var parentUserAccount = parentLink?.Parent?.Person?.Users?.FirstOrDefault();
        //        string parentPhone = parentUserAccount?.PhoneNumber ?? "No Number";

        //        string cleanFullName = $"{record.Student.Person.FirstName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

        //        dashboard.Students.Add(new StudentGridItemDto
        //        {
        //            StudentID = record.StudentId,
        //            StudentName = cleanFullName,
        //            Grade = gradeDisplay,
        //            Section = sectionDisplay, // رقم نقي تماماً
        //            Phone = parentPhone
        //        });
        //    }

        //    return dashboard;
        //}



        public async Task<StudentDirectoryDashboardDto> GetStudentDirectoryDashboardAsync(int managerPersonId, string? searchName, int page)
        {
            var dashboard = new StudentDirectoryDashboardDto();
            const int pageSize = 8;

            // 1. جلب الموجهين مع تضمين مدير القسم والشخص الخاص بمدير القسم لمنع الـ Shadow Properties تماماً
            var allSupervisors = await _supervisorRepo.GetAllWithIncludeAsync(
                s => s.DepartmentManager,
                s => s.DepartmentManager.Person
            );

            // تصحيح المطابقة باستخدام PersonID الصواب للسكريبت
            var activeSupervisorIds = allSupervisors
                .Where(s => s.DepartmentManager.PersonId == managerPersonId)
                .Select(s => s.SupervisorId)
                .ToList();

            // 2. جلب الصفوف الخاضعة لإشراف هؤلاء الموجهين (تصحيح المسميات لـ ClassRoomID و SupervisorID)
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

            // 4. سحب سجلات الطلاب وتطبيق شرط البحث بالاسم (مع تصحيح StudentID)
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

            // 5. حساب نسبة النجاح الأكاديمية للفصل الأول (تصحيح السجل ليكون StudentRecordID المطابق للسكريبت)
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

            var paginatedRecords = relevantStudentRecords
        .OrderBy(sr => sr.Student.Person.FirstName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

            // 7. الحل الجذري: جلب علاقات الأهل بشكل مسطح دون الدخول في تفرعات المستخدمين العكسية المعقدة
            var allStudentParents = await _studentParentRepo.GetAllWithIncludeAsync(
                sp => sp.Parent,
                sp => sp.Parent.Person
            );

            // جلب جدول المستخدمين (Users) بشكل منفصل في سطر واحد سريع لربط الهواتف عبر الذاكرة مباشرة
            var allUsers = await _userRepo.GetAllAsync();

            // 8. صياغة المخرجات النهائية بأمان تام
            foreach (var record in paginatedRecords)
            {
                string gradeDisplay = "-";
                int sectionDisplay = 0;

                var assignedClassLink = managedClassroomStudents.FirstOrDefault(cs => cs.StudentId == record.StudentId);
                if (assignedClassLink != null)
                {
                    var matchedRoom = managedClassRooms.FirstOrDefault(cr => cr.ClassRoomId == assignedClassLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        gradeDisplay = $"{matchedRoom.Grade.GradeNumber}th";
                        sectionDisplay = matchedRoom.Section;
                    }
                }

                // استخراج الهاتف بأمان عبر فحص متقاطع نقي في الذاكرة (In-Memory Lookup)
                var parentLink = allStudentParents.FirstOrDefault(sp => sp.StudentId == record.StudentId);
                string parentPhone = "No Number";

                if (parentLink?.Parent?.Person != null)
                {
                    // نذهب لجدول المستخدمين مباشرة ونبحث باستخدام الـ PersonID الصحيح والمؤكد للمشروع
                    var parentUserAccount = allUsers.FirstOrDefault(u => u.PersonId == parentLink.Parent.PersonId);
                    if (parentUserAccount != null)
                    {
                        parentPhone = parentUserAccount.PhoneNumber;
                    }
                }

                string cleanFullName = $"{record.Student.Person.FirstName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Students.Add(new StudentGridItemDto
                {
                    StudentId = record.StudentId,
                    StudentName = cleanFullName,
                    Grade = gradeDisplay,
                    Section = sectionDisplay,
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


        public async Task<string?> RegisterSupervisorWorkflowAsync(int managerPersonId, CreateSupervisorDto dto)
        {
            // 1. جلب سجل مدير القسم الحالي لمعرفة الـ DepartmentManagerID لربط المشرف به
            var managers = await _managerRepo.GetAllWithIncludeAsync();
            var activeManager = managers.FirstOrDefault(m => m.PersonId == managerPersonId);
            if (activeManager == null) return null;

            // 2. فتح ترانزكشن لحماية البيانات عبر الجداول الثلاثة
            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                // أ. توليد رقم الحساب تلقائياً باستخدام السيكوينس الموثق لديك بالداتابيز
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                // ب. بناء وحفظ سجل الشخص (People)
                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true, // تفعيل الحساب تلقائياً عند الإنشاء
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); // هنا يتولد الـ PersonID

                
                // د. بناء وحفظ سجل المستخدم (Users) مرتبطاً بالـ PersonID
                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 4, // رقم الدور 4 وهو المخصص للموجه (Supervisor) في نظام الأدوار لديك
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null,
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                // هـ. بناء وحفظ سجل الموجه (Supervisors) مرتبطاً بمدير القسم والشخص
                var newSupervisor = new Supervisor
                {
                    DepartmentManagerId = activeManager.DepartmentManagerId, // ربطه بمدير القسم الصانع للحساب
                    PersonId = newPerson.PersonId,
                    Salary = dto.Salary
                };
                await _supervisorRepo.AddAsync(newSupervisor);
                await _supervisorRepo.SaveChangesAsync();

                // 3. تأكيد وإغلاق المعاملة بنجاح
                await _classRoomRepo.CommitTransactionAsync();
                return generatedAccountNumber;
            }
            catch
            {
                // التراجع عن أي إدخال مجتزأ في حال حدوث أي خطأ تشغيلي لحماية سلامة البيانات
                await _classRoomRepo.RollbackTransactionAsync();
                return null;
            }
        }


        public async Task<string?> RegisterTeacherWorkflowAsync(CreateTeacherDto dto)
        {
            // 1. فتح ترانزكشن لحماية تتابع الجداول الثلاثة
            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                // أ. توليد رقم الحساب تلقائياً باستخدام السيكوينس المعتمد في السكريبت
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                // ب. بناء وحفظ سجل الشخص (People) ومطابقة PersonID بالحروف الكبيرة
                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true, // تفعيل حساب المعلم فور إنشائه
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); // توليد الـ PersonID

                // ج. بناء وحفظ سجل المستخدم (Users) - معرّف دور المعلم هو 2 في نظامك
                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = 2, // دور المعلم (Teacher)
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    HashPassword = null, // متروك فارغاً NULL ليتم تعيينه لاحقاً عند التفعيل
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                // د. بناء وحفظ سجل المعلم (Teachers) مرتبطاً بالـ PersonID
                var newTeacher = new Teacher
                {
                    PersonId = newPerson.PersonId,
                    WeeklyClasses = dto.WeeklyClasses,
                    SalaryPerClass = dto.SalaryPerClass
                };
                await _teacherRepo.AddAsync(newTeacher);
                await _teacherRepo.SaveChangesAsync();

                // 2. تأكيد وإغلاق المعاملة بنجاح
                await _classRoomRepo.CommitTransactionAsync();

                // إرجاع رقم الحساب المتولد لعرضه في الواجهة
                return generatedAccountNumber;
            }
            catch
            {
                // التراجع الشامل لحماية سلامة الجداول في حال حدوث أي خطأ
                await _classRoomRepo.RollbackTransactionAsync();
                return null;
            }
        }

        public async Task<bool> CreateNextSectionAutomatedAsync(CreateAutomaticClassRoomDto dto)
        {
            // 1. جلب جميع الغرف الصفية المسجلة مسبقاً لهذا الصف (GradeID) المحدد
            var existingClassRooms = await _classRoomRepo.GetAllWithIncludeAndFilterAsync(
                cr => cr.GradeId == dto.GradeID
            );

            byte nextSectionNumber = 1; // القيمة الافتراضية إذا كان الصف جديداً كلياً ولا يملك أي شعبة

            if (existingClassRooms.Any())
            {
                // 2. تطبيق الاستراتيجية الأفضل: البحث عن أعلى رقم شعبة موجود حالياً وزيادته بـ 1
                var maxSection = existingClassRooms.Max(cr => cr.Section);
                nextSectionNumber = (byte)(maxSection + 1);
            }

            // 3. تحديد السنة الدراسية الحالية تلقائياً (StartYear) بناءً على تاريخ السيرفر الحالي
            short currentAcademicYear = (short)DateTime.Today.Year;

            // 4. بناء كائن الشعبة الجديدة وحفظه في الداتابيز (SupervisorID يترك NULL حتى يتم فرزه لاحقاً)
            var newClassRoom = new ClassRoom
            {
                GradeId = dto.GradeID,
                Section = nextSectionNumber, // الرقم التلقائي الذكي (مثل 4)
                SupervisorId = null,
                StartYear = currentAcademicYear
            };

            await _classRoomRepo.AddAsync(newClassRoom);
            await _classRoomRepo.SaveChangesAsync();

            return true;
        }

        public async Task<SupervisorDetailsDto> GetSupervisorByIdAsync(int managerPersonId, int supervisorId)
        {
            var managers = await _managerRepo.GetAllWithIncludeAsync();
            var activeManager = managers.FirstOrDefault(m => m.PersonId == managerPersonId);
            if (activeManager == null)
                throw new UnauthorizedAccessException("حساب مدير القسم غير صالح.");

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(
                s => s.SupervisorId == supervisorId,
                s => s.Person,
                s => s.DepartmentManager
            );
            var supervisor = supervisors.FirstOrDefault();
            if (supervisor == null)
                throw new KeyNotFoundException("الموجه المطلوب غير موجود في النظام.");

            if (supervisor.DepartmentManagerId != activeManager.DepartmentManagerId)
                throw new UnauthorizedAccessException("ليس لديك صلاحية للوصول إلى بيانات موجه لا يتبع لقسمك.");

            var allUsers = await _userRepo.GetAllAsync();
            var userAccount = allUsers.FirstOrDefault(u => u.PersonId == supervisor.PersonId);

            var allClassRooms = await _classRoomRepo.GetAllAsync();
            int assignedSections = allClassRooms.Count(cr => cr.SupervisorId == supervisorId);

            var allTeacherSupervisors = await _teacherSupervisorRepo.GetAllAsync();
            int supervisedTeachers = allTeacherSupervisors.Count(ts => ts.SupervisorId == supervisorId);

            return new SupervisorDetailsDto
            {
                SupervisorId = supervisor.SupervisorId,
                PersonId = supervisor.PersonId,
                FirstName = supervisor.Person.FirstName,
                SecondName = supervisor.Person.SecondName,
                LastName = supervisor.Person.LastName,
                FullName = $"{supervisor.Person.FirstName} {supervisor.Person.SecondName} {supervisor.Person.LastName}".Replace("  ", " ").Trim(),
                DateOfBirth = supervisor.Person.DateOfBirth,
                Gender = supervisor.Person.Gender,
                PhoneNumber = userAccount?.PhoneNumber ?? string.Empty,
                Email = userAccount?.Email,
                AccountNumber = userAccount?.AccountNumber ?? string.Empty,
                Salary = supervisor.Salary ?? 0m,
                IsActive = supervisor.Person.IsActive,
                AssignedSectionsCount = assignedSections,
                SupervisedTeachersCount = supervisedTeachers
            };
        }

        // =========================================================================
        // تعديل بيانات الموجه (Update Supervisor)
        // =========================================================================
        public async Task<SupervisorDetailsDto> UpdateSupervisorAsync(int managerPersonId, int supervisorId, UpdateSupervisorDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "بيانات التعديل مطلوبة.");

            var managers = await _managerRepo.GetAllWithIncludeAsync();
            var activeManager = managers.FirstOrDefault(m => m.PersonId == managerPersonId);
            if (activeManager == null)
                throw new UnauthorizedAccessException("حساب مدير القسم غير صالح.");

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(
                s => s.SupervisorId == supervisorId,
                s => s.Person
            );
            var supervisor = supervisors.FirstOrDefault();
            if (supervisor == null)
                throw new KeyNotFoundException("الموجه المطلوب غير موجود في النظام.");

            if (supervisor.DepartmentManagerId != activeManager.DepartmentManagerId)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتعديل بيانات موجه لا يتبع لقسمك.");

            string cleanPhone = dto.PhoneNumber.Trim();
            string? cleanEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower();

            // 1. التحقق من عدم تكرار رقم الهاتف مع حساب مستخدم آخر في النظام
            var allUsers = await _userRepo.GetAllAsync();
            var phoneTaken = allUsers.Any(u => u.PhoneNumber == cleanPhone && u.PersonId != supervisor.PersonId);
            if (phoneTaken)
                throw new InvalidOperationException($"رقم الهاتف '{cleanPhone}' مسجل مسبقاً لمستخدم آخر في النظام.");

            // 2. التحقق من عدم تكرار البريد الإلكتروني مع حساب مستخدم آخر إن وُجد
            if (!string.IsNullOrEmpty(cleanEmail))
            {
                var emailTaken = allUsers.Any(u => !string.IsNullOrEmpty(u.Email) && u.Email.ToLower() == cleanEmail && u.PersonId != supervisor.PersonId);
                if (emailTaken)
                    throw new InvalidOperationException($"البريد الإلكتروني '{cleanEmail}' مسجل مسبقاً لمستخدم آخر في النظام.");
            }

            // 3. تنفيذ التعديلات المتكاملة داخل ترانزكشن لحماية سلامة الجداول (Person + User + Supervisor)
            await _classRoomRepo.BeginTransactionAsync();
            try
            {
                // أ. تحديث بيانات الشخص (People)
                supervisor.Person.FirstName = dto.FirstName.Trim();
                supervisor.Person.SecondName = dto.SecondName.Trim();
                supervisor.Person.LastName = dto.LastName.Trim();
                supervisor.Person.DateOfBirth = dto.DateOfBirth;
                supervisor.Person.Gender = dto.Gender;
                supervisor.Person.IsActive = dto.IsActive;
                _personRepo.UpdateAsync(supervisor.Person);
                await _personRepo.SaveChangesAsync();

                // ب. تحديث بيانات حساب المستخدم (Users)
                var userAccount = allUsers.FirstOrDefault(u => u.PersonId == supervisor.PersonId);
                if (userAccount != null)
                {
                    userAccount.PhoneNumber = cleanPhone;
                    userAccount.Email = cleanEmail;
                    _userRepo.UpdateAsync(userAccount);
                    await _userRepo.SaveChangesAsync();
                }

                // ج. تحديث بيانات الموجه المالية (Supervisors)
                supervisor.Salary = dto.Salary;
                _supervisorRepo.UpdateAsync(supervisor);
                await _supervisorRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();

                return await GetSupervisorByIdAsync(managerPersonId, supervisorId);
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                throw;
            }
        }

        // =========================================================================
        // حذف الموجه بأمان مع التحقق الشامل من كافة المسؤوليات والارتباطات (Delete Supervisor)
        // =========================================================================
        public async Task<bool> DeleteSupervisorAsync(int managerPersonId, int supervisorId)
        {
            var managers = await _managerRepo.GetAllWithIncludeAsync();
            var activeManager = managers.FirstOrDefault(m => m.PersonId == managerPersonId);
            if (activeManager == null)
                throw new UnauthorizedAccessException("حساب مدير القسم غير صالح.");

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(
                s => s.SupervisorId == supervisorId,
                s => s.Person
            );
            var supervisor = supervisors.FirstOrDefault();
            if (supervisor == null)
                throw new KeyNotFoundException("الموجه المطلوب غير موجود في النظام.");

            if (supervisor.DepartmentManagerId != activeManager.DepartmentManagerId)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لحذف موجه لا يتبع لقسمك.");

            // 1. الفحص الصارم للشعب والغرف الصفية المرتبطة بإشراف هذا الموجه
            var assignedClassrooms = await _classRoomRepo.GetAllWithIncludeAndFilterAsync(
                cr => cr.SupervisorId == supervisorId,
                cr => cr.Grade
            );
            if (assignedClassrooms.Any())
            {
                var classNames = string.Join("، ", assignedClassrooms.Select(c => $"الصف {c.Grade?.GradeNumber ?? c.GradeId} شعبة {c.Section}"));
                throw new InvalidOperationException($"لا يمكن حذف الموجه لوجود ({assignedClassrooms.Count()}) شعبة صفية مرتبطة بإشرافه حالياً: ({classNames}). يرجى نقل أو إلغاء إشراف هذه الشعب قبل الحذف.");
            }

            // 2. الفحص الصارم لعلاقات إشراف المعلمين
            var assignedTeachers = await _teacherSupervisorRepo.GetAllWithIncludeAndFilterAsync(
                ts => ts.SupervisorId == supervisorId,
                ts => ts.Teacher,
                ts => ts.Teacher.Person
            );
            if (assignedTeachers.Any())
            {
                var teacherNames = string.Join("، ", assignedTeachers.Take(4).Select(t => $"{t.Teacher.Person.FirstName} {t.Teacher.Person.LastName}".Trim()));
                throw new InvalidOperationException($"لا يمكن حذف الموجه لأنه يشرف حالياً على ({assignedTeachers.Count()}) معلم: ({teacherNames}). يرجى فك ارتباط إشراف المعلمين أولاً.");
            }

            // 3. فحص غرف المحادثات النشطة مع أولياء الأمور
            //var activeChats = await _chatRoomRepo.GetAllWithIncludeAndFilterAsync(
            //    cr => cr.SupervisorPersonId == supervisor.PersonId && cr.IsActive
            //);
            //if (activeChats.Any())
            //{
            //    throw new InvalidOperationException($"لا يمكن حذف الموجه لوجود ({activeChats.Count()}) غرفة محادثة نشطة مع أولياء الأمور. يرجى أرشفة أو إغلاق المحادثات قبل تنفيذ الحذف.");
            //}

            // 4. تنفيذ الحذف الآمن ضمن ترانزكشن
            await _classRoomRepo.BeginTransactionAsync();
            try
            {
                int personId = supervisor.PersonId;

                // حذف سجل الموجه
                _supervisorRepo.Delete(supervisor);
                await _supervisorRepo.SaveChangesAsync();

                // حذف حساب المستخدم
                var allUsers = await _userRepo.GetAllAsync();
                var userAccount = allUsers.FirstOrDefault(u => u.PersonId == personId);
                if (userAccount != null)
                {
                    _userRepo.Delete(userAccount);
                    await _userRepo.SaveChangesAsync();
                }

                // إلغاء تفعيل حساب الشخص لضمان عدم وجود بيانات متضاربة
                var person = await _personRepo.GetByIdAsync(personId);
                if (person != null)
                {
                    person.IsActive = false;
                    _personRepo.UpdateAsync(person);
                    await _personRepo.SaveChangesAsync();
                }

                await _classRoomRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
