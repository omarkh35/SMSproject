using BLL.EntitiesDTOS.Supervisor;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classStudentRepo;
        private readonly IBaseRepositories<StudentAttendance> _studentAttendanceRepo;
        IBaseRepositories<ToDoTask> _taskRepo;
        IBaseRepositories<ClassroomTeacher> _classTeacherRepo;
        IBaseRepositories<TeacherAttendance> _teacherAttendanceRepo;
        IBaseRepositories<Announcement> _announcementRepo;
        IBaseRepositories<AnnouncementClassroom> _announcementClassroomRepo;
        IBaseRepositories<Mark> _markRepo;
        IBaseRepositories<Schedule> _scheduleRepo;
        IBaseRepositories<ExamSchedule> _examScheduleRepo;
        IBaseRepositories<StudentRecord> _studentRecordRepo;
        IBaseRepositories<StudentParent> _studentParentRepo;
        IBaseRepositories<Teacher> _teacherRepo;

        public SupervisorService(IBaseRepositories<ClassRoom> classRoomRepo, IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<ClassroomStudent> classStudentRepo,
            IBaseRepositories<StudentAttendance> studentAttendanceRepo, IBaseRepositories<ToDoTask> taskRepo,
            IBaseRepositories<ClassroomTeacher> classTeacherRepo, IBaseRepositories<TeacherAttendance> teacherAttendanceRepo,
            IBaseRepositories<Announcement> announcementRepo,IBaseRepositories<AnnouncementClassroom> announcementClassroomRepo,
            IBaseRepositories<Mark> markRepo,IBaseRepositories<Schedule> scheduleRepo,
        IBaseRepositories<StudentRecord> studentRecordRepo,
        IBaseRepositories<ExamSchedule> examScheduleRepo, IBaseRepositories<StudentParent> studentParentRepo,
        IBaseRepositories<Teacher> teacherRepo)
        {
            _classRoomRepo = classRoomRepo;
            _supervisorRepo = supervisorRepo;
            _classStudentRepo = classStudentRepo;
            _studentAttendanceRepo = studentAttendanceRepo;
            _taskRepo = taskRepo;
            _classTeacherRepo = classTeacherRepo;
            _teacherAttendanceRepo = teacherAttendanceRepo;
            _announcementRepo = announcementRepo;
            _announcementClassroomRepo = announcementClassroomRepo;
            _taskRepo = taskRepo;
            _examScheduleRepo = examScheduleRepo;
            _markRepo = markRepo;
            _scheduleRepo = scheduleRepo;
            _studentParentRepo = studentParentRepo;
            _teacherRepo = teacherRepo;

        }

        public async Task<SupervisorMainDashboardDto> GetMainDashboardAsync(int supervisorPersonId)
        {
            var dashboard = new SupervisorMainDashboardDto();
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // 1. Get the current Supervisor's tracking primary key ID context
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return dashboard;

            // 2. Load rooms assigned directly to this supervisor
            var rooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRooms = rooms.Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId).ToList();
            var supervisedRoomIds = supervisedRooms.Select(cr => cr.ClassRoomId).ToList();

            dashboard.ClassesCount = supervisedRooms.Count;

            // Populate Dropdown collection mapping
            foreach (var r in supervisedRooms)
            {
                dashboard.SupervisedClasses.Add(new SupervisorClassDropdownDto
                {
                    ClassRoomID = r.ClassRoomId,
                    // Match the visual text casing strings in your screenshot layout ("SEVENTH - FIRST")
                    ClassDisplayName = $"Grade {r.Grade.GradeNumber} - Section {r.Section}"
                });
            }

            // 3. Collect students operating inside those rooms
            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.Student, cs => cs.Student.Person);
            var managedStudents = classroomStudents.Where(cs => supervisedRoomIds.Contains(cs.ClassRoomId)).ToList();
            var managedStudentIds = managedStudents.Select(cs => cs.StudentId).ToList();

            dashboard.TotalStudentsCount = managedStudentIds.Distinct().Count();

            // 4. Evaluate real-time Attendance logs for today
            var todayAttendance = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                sa => sa.AttendanceDate == todayDate && managedStudentIds.Contains(sa.StudentId)
            );

            dashboard.AbsentTodayCount = todayAttendance.Count(sa => sa.Status == 2); // 2 = Absent
                                                                                      // PresentCount calculation includes simple present metrics + late arrivals
            dashboard.PresentTodayCount = todayAttendance.Count(sa => sa.Status == 1 || sa.Status == 3);

            // 5. Hydrate the "Absent Today" Global Real-time Exception Grid List Feed View
            var alertRecords = todayAttendance.Where(sa => sa.Status == 2 || sa.Status == 3).ToList();
            foreach (var alert in alertRecords)
            {
                var studentInfo = managedStudents.FirstOrDefault(cs => cs.StudentId == alert.StudentId);
                if (studentInfo == null) continue;

                dashboard.ExceptionFeed.Add(new AbsentTodayGridItemDto
                {
                    FullName = $"{studentInfo.Student.Person.FirstName} {studentInfo.Student.Person.LastName}",
                    ClassName = $"Grade {studentInfo.ClassRoom.Grade.GradeNumber}",
                    SectionName = $"Section {studentInfo.ClassRoom.Section}",
                    Status = alert.Status == 2 ? "ABSENT" : "LATE"
                });
            }

            return dashboard;
        }

        public async Task<ClassRollCallDto?> GetClassroomRollCallAsync(int supervisorPersonId, int classRoomId)
        {
            // Validation: Verify this room actually falls under this specific supervisor's scope line
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return null;

            var targetRoom = await _classRoomRepo.GetByIdAsync(classRoomId);
            if (targetRoom == null || targetRoom.SupervisorId != activeSupervisor.SupervisorId) return null;

            var rollCall = new ClassRollCallDto();
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // Get all students enrolled in this target classroom
            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId,
                cs => cs.Student,
                cs => cs.Student.Person
            );

            var studentIdsInClass = classroomStudents.Select(cs => cs.StudentId).ToList();

            // Check if attendance transactions have been recorded for this classroom block today
            var classAttendance = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                sa => sa.ClassRoomId == classRoomId && sa.AttendanceDate == todayDate
            );

            if (classAttendance.Any())
            {
                rollCall.IsAttendanceTaken = true;
                rollCall.StatusMessage = "Attendance taken";
            }
            else
            {
                rollCall.IsAttendanceTaken = false;
                // Matches exact textual notification business rule requirement specified
                rollCall.StatusMessage = "Attendance was not taken yet";
            }

            // Hydrate list for grid checkboxes display fields mapping
            foreach (var cs in classroomStudents)
            {
                var todaysStatusRecord = classAttendance.FirstOrDefault(sa => sa.StudentId == cs.StudentId);
                string statusString = "Not Set";

                if (todaysStatusRecord != null)
                {
                    statusString = todaysStatusRecord.Status switch
                    {
                        1 => "Present",
                        2 => "Absent",
                        3 => "Late",
                        4 => "Excused",
                        _ => "Not Set"
                    };
                }

                rollCall.Students.Add(new RollCallStudentItemDto
                {
                    StudentID = cs.StudentId,
                    FullName = $"{cs.Student.Person.FirstName} {cs.Student.Person.SecondName} {cs.Student.Person.LastName}".Replace("  ", " ").Trim(),
                    CurrentStatus = statusString
                });
            }

            return rollCall;
        }


        public async Task<IEnumerable<SupervisorTaskDto>> GetTodayTasksAsync(int supervisorPersonId)
        {
            var todayDate = DateTime.UtcNow.Date;

            var allTasks = await _taskRepo.GetAllWithIncludeAndFilterAsync(
                t => t.AssignedPersonID == supervisorPersonId && t.CreatedAt.Date == todayDate
            );

            return allTasks.Select(t => new SupervisorTaskDto
            {
                TaskID = t.TaskID,
                TaskDescription = t.TaskDescription,
                IsDone = t.IsDone,
                DueDate = t.DueDate,
                ClassRoomID = t.ClassRoomID,
                PriorityLevel = t.PriorityLevel
            });
        }

        public async Task<bool> CreateTaskAsync(int supervisorPersonId, CreateTaskDto dto)
        {
            var task = new ToDoTask
            {
                AssignedPersonID = supervisorPersonId,
                TaskDescription = dto.TaskDescription,
                DueDate = dto.DueDate,
                ClassRoomID = dto.ClassRoomID, 
                PriorityLevel = dto.PriorityLevel,
                IsDone = false,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepo.AddAsync(task);
            await _taskRepo.SaveChangesAsync();
            return true;
        }

        
        public async Task<bool> ToggleTaskAsync(int supervisorPersonId, long taskId)
        {
            var tasks = await _taskRepo.GetAllWithIncludeAndFilterAsync(
                t => t.TaskID == taskId && t.AssignedPersonID == supervisorPersonId
            );
            var targetTask = tasks.FirstOrDefault();
            if (targetTask == null) return false;

            targetTask.IsDone = !targetTask.IsDone; 

            _taskRepo.UpdateAsync(targetTask);
            await _taskRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int supervisorPersonId, long taskId)
        {
            var tasks = await _taskRepo.GetAllWithIncludeAndFilterAsync(
                t => t.TaskID == taskId && t.AssignedPersonID == supervisorPersonId
            );
            var targetTask = tasks.FirstOrDefault();
            if (targetTask == null) return false;

            _taskRepo.Delete(targetTask);
            await _taskRepo.SaveChangesAsync();
            return true;
        }

        public async Task<AttendanceSheetLoadDto?> LoadAttendanceSheetAsync(int supervisorPersonId, int classRoomId)
        {
            // 1. Validation Hierarchy Guard
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null || await VerifyOversightAsync(activeSupervisor.SupervisorId, classRoomId) == false)
                return null;

            var sheet = new AttendanceSheetLoadDto();
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // 2. Load Supervised Students for this specific Class
            var classStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId, cs => cs.Student, cs => cs.Student.Person
            );

            // 3. Load Supervised Teachers linked to this specific Class
            var classTeachers = await _classTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.ClassRoomId == classRoomId, ct => ct.Teacher, ct => ct.Teacher.Person
            );

            // 4. Fetch existing real-time logs for today
            var studentAttendanceToday = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                sa => sa.ClassRoomId == classRoomId && sa.AttendanceDate == todayDate
            );

            var teacherIdsInClass = classTeachers.Select(ct => ct.TeacherId).ToList();
            var teacherAttendanceToday = await _teacherAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                ta => ta.AttendanceDate == todayDate && teacherIdsInClass.Contains(ta.TeacherId)
            );

            sheet.IsAlreadyRecordedToday = studentAttendanceToday.Any() || teacherAttendanceToday.Any();

            // 5. Hydrate Student Grid Rows (with pre-saved state if existing)
            foreach (var cs in classStudents)
            {
                var existingLog = studentAttendanceToday.FirstOrDefault(sa => sa.StudentId == cs.StudentId);
                sheet.Students.Add(new StudentAttendanceRowDto
                {
                    StudentID = cs.StudentId,
                    FullName = $"{cs.Student.Person.FirstName} {cs.Student.Person.LastName}",
                    Status = existingLog?.Status ?? 1, // Default to 1 (Present) if clean load
                    Note = existingLog?.Notes ?? string.Empty
                });
            }

            // 6. Hydrate Teacher Grid Rows (with pre-saved state if existing)
            foreach (var ct in classTeachers)
            {
                var existingLog = teacherAttendanceToday.FirstOrDefault(ta => ta.TeacherId == ct.TeacherId);
                sheet.Teachers.Add(new TeacherAttendanceRowDto
                {
                    TeacherID = ct.TeacherId,
                    FullName = $"{ct.Teacher.Person.FirstName} {ct.Teacher.Person.LastName}",
                    Status = existingLog?.Status ?? 1, // Default to 1 (Present) if clean load
                    Note = existingLog?.Notes ?? string.Empty
                });
            }

            return sheet;
        }

        public async Task<bool> SaveAttendanceSheetWorkflowAsync(int supervisorPersonId, SaveAttendanceSheetDto dto)
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null || await VerifyOversightAsync(activeSupervisor.SupervisorId, dto.ClassRoomID) == false)
                return false;

            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // Use transaction engine via Base Repo to safeguard mass entry writes
            var transaction = await _classRoomRepo.BeginTransactionAsync();
            try
            {
                // === PROCESS STUDENTS (UPSERT WORKFLOW) ===
                var existingStudentLogs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    sa => sa.ClassRoomId == dto.ClassRoomID && sa.AttendanceDate == todayDate
                );

                foreach (var sDto in dto.StudentRecords)
                {
                    var matchedLog = existingStudentLogs.FirstOrDefault(sa => sa.StudentId == sDto.StudentID);
                    if (matchedLog != null)
                    {
                        // UPDATE
                        matchedLog.Status = sDto.Status;
                        matchedLog.Notes = sDto.Note;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _studentAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
                        // INSERT
                        var newLog = new StudentAttendance
                        {
                            StudentId = sDto.StudentID,
                            ClassRoomId = dto.ClassRoomID,
                            AttendanceDate = todayDate,
                            Status = sDto.Status,
                            Notes = sDto.Note,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _studentAttendanceRepo.AddAsync(newLog);
                    }
                }
                await _studentAttendanceRepo.SaveChangesAsync();

                // === PROCESS TEACHERS (UPSERT WORKFLOW) ===
                var teacherIds = dto.TeacherRecords.Select(t => t.TeacherID).ToList();
                var existingTeacherLogs = await _teacherAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    ta => ta.AttendanceDate == todayDate && teacherIds.Contains(ta.TeacherId)
                );

                foreach (var tDto in dto.TeacherRecords)
                {
                    var matchedLog = existingTeacherLogs.FirstOrDefault(ta => ta.TeacherId == tDto.TeacherID);
                    if (matchedLog != null)
                    {
                        // UPDATE
                        matchedLog.Status = tDto.Status;
                        matchedLog.Notes = tDto.Note;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _teacherAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
                        // INSERT
                        var newLog = new TeacherAttendance
                        {
                            TeacherId = tDto.TeacherID,
                            AttendanceDate = todayDate,
                            Status = tDto.Status,
                            Notes = tDto.Note,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _teacherAttendanceRepo.AddAsync(newLog);
                    }
                }
                await _teacherAttendanceRepo.SaveChangesAsync();

                await _classRoomRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return false;
            }
        }

        // Internal reusable architecture guard
        private async Task<bool> VerifyOversightAsync(int supervisorId, int classRoomId)
        {
            var room = await _classRoomRepo.GetByIdAsync(classRoomId);
            return room != null && room.SupervisorId == supervisorId;
        }

        public async Task<AnnouncementManagementPageDto> LoadAnnouncementsPanelAsync(int senderPersonId)
        {
            var pageData = new AnnouncementManagementPageDto();

            // 1. Hydrate the Dropdown options list
            pageData.TargetOptions.Add(new AnnouncementTargetDropdownDto { ClassRoomID = null, DisplayName = "All classes" });

            var classes = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            foreach (var cr in classes)
            {
                pageData.TargetOptions.Add(new AnnouncementTargetDropdownDto
                {
                    ClassRoomID = cr.ClassRoomId,
                    DisplayName = $"{cr.Grade.GradeNumber}th Grade / Class {cr.Section}" // Matches UI dropdown look
                });
            }

            // 2. Hydrate Published History List at the bottom (Filtered to only show announcements sent by THIS user)
            var myAnnouncements = await _announcementRepo.GetAllWithIncludeAndFilterAsync(
                a => a.SenderPersonId == senderPersonId,
                a => a.AnnouncementClassrooms
            );

            foreach (var ann in myAnnouncements.OrderByDescending(a => a.CreatedAt))
            {
                string targetDisplay = "All Classes";
                if (!ann.IsGeneral && ann.AnnouncementClassrooms.Any())
                {
                    var firstTargetId = ann.AnnouncementClassrooms.First().ClassRoomId;
                    var matchedClass = classes.FirstOrDefault(c => c.ClassRoomId == firstTargetId);
                    if (matchedClass != null)
                    {
                        targetDisplay = $"{matchedClass.Grade.GradeNumber}th Grade - Section {matchedClass.Section}";
                        if (ann.AnnouncementClassrooms.Count > 1) targetDisplay += $" (+{ann.AnnouncementClassrooms.Count - 1} more)";
                    }
                }

                pageData.MyPublishedAnnouncements.Add(new PublishedAnnouncementItemDto
                {
                    AnnouncementID = ann.AnnouncementId,
                    Title = ann.Title,
                    Content = ann.AnnouncementBody,
                    TargetAudienceDisplay = targetDisplay,
                    CreatedAt = ann.CreatedAt
                });
            }

            return pageData;
        }

        public async Task<bool> PublishAnnouncementAsync(int senderPersonId, CreateAnnouncementRequestDto dto)
        {
            var transaction = await _announcementRepo.BeginTransactionAsync();
            try
            {
                // 1. Insert into main Announcements table
                var newAnnouncement = new Announcement
                {
                    SenderPersonId = senderPersonId,
                    Title = dto.Title,
                    AnnouncementBody = dto.Content,
                    IsGeneral = dto.IsGeneral,
                    CreatedAt = DateTime.UtcNow
                };
                await _announcementRepo.AddAsync(newAnnouncement);
                await _announcementRepo.SaveChangesAsync(); // Generates AnnouncementID

                // 2. If targeted to specific classes, populate the link junction table
                if (!dto.IsGeneral && dto.TargetClassRoomIDs.Any())
                {
                    foreach (var classRoomId in dto.TargetClassRoomIDs)
                    {
                        var link = new AnnouncementClassroom
                        {
                            AnnouncementId = newAnnouncement.AnnouncementId,
                            ClassRoomId = classRoomId
                        };
                        await _announcementClassroomRepo.AddAsync(link);
                    }
                    await _announcementClassroomRepo.SaveChangesAsync();
                }

                await _announcementRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _announcementRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeleteAnnouncementAsync(int senderPersonId, int announcementId)
        {
            var records = await _announcementRepo.GetAllWithIncludeAndFilterAsync(
                a => a.AnnouncementId == announcementId && a.SenderPersonId == senderPersonId,
                a => a.AnnouncementClassrooms
            );
            var target = records.FirstOrDefault();
            if (target == null) return false;

            // Cascade delete any mapped child class targets links safely first inside transaction scope if your DB context doesn't handle cascades natively
            foreach (var childLink in target.AnnouncementClassrooms.ToList())
            {
                _announcementClassroomRepo.Delete(childLink);
            }
            await _announcementClassroomRepo.SaveChangesAsync();

            _announcementRepo.Delete(target);
            await _announcementRepo.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<SupervisorClassCardDto>> GetSupervisedClassroomsDirectoryAsync(int supervisorPersonId)
        {
            var classCards = new List<SupervisorClassCardDto>();
            var currentYear = (short)DateTime.UtcNow.Year;

            // 1. Locate the supervisor profile linked to this identity
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return classCards;

            // 2. Fetch all components via generic repositories to process mappings
            var allRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRooms = allRooms.Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId).ToList();
            var supervisedRoomIds = supervisedRooms.Select(cr => cr.ClassRoomId).ToList();

            var allClassroomStudents = await _classStudentRepo.GetAllAsync();
            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.ExamType);

            // Fetch image-based programs paths we designed earlier
            var allSchedules = await _scheduleRepo.GetAllAsync();
            var allExamSchedules = await _examScheduleRepo.GetAllAsync();

            // 3. Process calculations loop per Supervised Class Card
            foreach (var room in supervisedRooms)
            {
                // Calculate Number of Students
                var studentIdsInClass = allClassroomStudents
                    .Where(cs => cs.ClassRoomId == room.ClassRoomId)
                    .Select(cs => cs.StudentId)
                    .ToList();

                // Calculate Class Average based on all approved marks recorded inside this specific room
                var classApprovedMarks = allMarks
                    .Where(m => m.IsApproved && studentIdsInClass.Contains(m.StudentRecordId))
                    .ToList();

                string averageDisplay = "N/A";
                if (classApprovedMarks.Any())
                {
                    // Convert everything out of a uniform scale (percentage) for the UI card layout
                    double totalPercentageSum = classApprovedMarks
                        .Sum(m => (double)(m.MarkValue / m.FullMark) * 100);

                    double average = totalPercentageSum / classApprovedMarks.Count;
                    averageDisplay = $"{Math.Round(average)} %";
                }

                // Extract Schedule Images File Paths
                // ScheduleType 1 = ClassRoom Weekly Program, ReferenceID = ClassRoomID
                var weeklyProg = allSchedules.FirstOrDefault(s => s.ScheduleType == 1 && s.ReferenceId == room.ClassRoomId);

                // ExamSchedules match via GradeID for the current academic session year context
                var examProg = allExamSchedules.FirstOrDefault(es => es.GradeId == room.GradeId && es.AcademicYear == currentYear);

                classCards.Add(new SupervisorClassCardDto
                {
                    ClassRoomID = room.ClassRoomId,
                    ClassName = $"{room.Grade.GradeNumber}th grade / {GetSectionNameWord(room.Section)}",
                    NumberOfStudents = studentIdsInClass.Count,
                    ClassAverage = averageDisplay, // Maps dynamically to "79 %" layout style
                    WeeklyWorkScheduleUrl = weeklyProg?.ImagePath ?? "uploads/schedules/default_schedule.png",
                    SemesterExamScheduleUrl = examProg?.ImagePath ?? "uploads/schedules/default_exams.png"
                });
            }

            return classCards;
        }

        // Optional layout string cleaner helper method
        private string GetSectionNameWord(byte sectionNumber)
        {
            return sectionNumber switch
            {
                1 => "first",
                2 => "second",
                3 => "third",
                _ => $"section {sectionNumber}"
            };
        }

        public async Task<IEnumerable<SupervisorStudentGridDto>> GetMyStudentsDirectoryAsync(int supervisorPersonId, int? classRoomId, string searchTerm)
        {
            var studentList = new List<SupervisorStudentGridDto>();

            // 1. Locate supervisor primary key context
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return studentList;

            // 2. Fetch supervised classrooms
            var rooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRoomIds = rooms
                .Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId)
                .Select(cr => cr.ClassRoomId)
                .ToList();

            // If the UI passes a specific ClassRoomID filter, target only that class
            if (classRoomId.HasValue && supervisedRoomIds.Contains(classRoomId.Value))
            {
                supervisedRoomIds = new List<int> { classRoomId.Value };
            }

            // 3. Gather student tracking links inside those targeted rooms
            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(
                cs => cs.Student,
                cs => cs.Student.Person
            );

            var filteredClassStudents = classroomStudents
                .Where(cs => supervisedRoomIds.Contains(cs.ClassRoomId))
                .ToList();

            // Apply text search bar filter if provided (matches name arrays)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                filteredClassStudents = filteredClassStudents
                    .Where(cs => cs.Student.Person.FirstName.ToLower().Contains(searchTerm) ||
                                 cs.Student.Person.LastName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            var managedStudentIds = filteredClassStudents.Select(cs => cs.StudentId).ToList();

            // 4. Pre-load master transaction layers for bulk optimization loop calculations
            var allMarks = await _markRepo.GetAllAsync();
            var allAttendance = await _studentAttendanceRepo.GetAllAsync();

            // 5. Map Rows and compute analytical columns metrics
            foreach (var cs in filteredClassStudents)
            {
                // Metric A: Compute GPA (Average of all APPROVED exam marks)
                var studentMarks = allMarks.Where(m => m.IsApproved && m.StudentRecordId == cs.StudentId).ToList();
                string gpaDisplay = "0 %";
                if (studentMarks.Any())
                {
                    double average = studentMarks.Average(m => (double)(m.MarkValue / m.FullMark) * 100);
                    gpaDisplay = $"{Math.Round(average)} %";
                }

                // Metric B: Compute Attendance Rate (Present / Total Days Taken)
                var studentHistory = allAttendance.Where(sa => sa.StudentId == cs.StudentId).ToList();
                string attendanceDisplay = "100 %";
                if (studentHistory.Any())
                {
                    // Status: 1 = Present, 3 = Late (both count as attending school)
                    int attendedDays = studentHistory.Count(sa => sa.Status == 1 || sa.Status == 3);
                    double rate = ((double)attendedDays / studentHistory.Count) * 100;
                    attendanceDisplay = $"{Math.Round(rate)} %";
                }

                var matchedRoom = rooms.FirstOrDefault(r => r.ClassRoomId == cs.ClassRoomId);

                studentList.Add(new SupervisorStudentGridDto
                {
                    StudentID = cs.StudentId,
                    FullName = $"{cs.Student.Person.FirstName} {cs.Student.Person.SecondName} {cs.Student.Person.LastName}".Replace("  ", " ").Trim(),
                    ClassName = matchedRoom != null ? $"{matchedRoom.Grade.GradeNumber}th" : "N/A",
                    SectionName = matchedRoom != null ? GetSectionNameWord(matchedRoom.Section) : "N/A",
                    GPA = gpaDisplay,              // e.g. "70 %" as shown in your visual design layout
                    AttendanceRate = attendanceDisplay // e.g. "86 %" as shown in your visual design layout
                });
            }

            return studentList;
        }



        public async Task<StudentDetailsPageDto?> GetStudentDetailedProfileAsync(int studentId, int month, int year)
        {
            // 1. Fetch Student Records info with full model hierarchy tracking
            var allStudentRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
            var targetRecord = allStudentRecords.FirstOrDefault(sr => sr.StudentId == studentId);
            if (targetRecord == null) return null;

            var pageData = new StudentDetailsPageDto { StudentID = studentId };
            pageData.FullName = $"{targetRecord.Student.Person.FirstName} {targetRecord.Student.Person.LastName}";

            // 2. Resolve Class and Section display string names
            var classLinks = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.ClassRoom, cs => cs.ClassRoom.Grade);
            var activeLink = classLinks.FirstOrDefault(cs => cs.StudentId == studentId);
            pageData.ClassAndSection = activeLink != null
                ? $"{activeLink.ClassRoom.Grade.GradeNumber}th / {GetSectionNameWord(activeLink.ClassRoom.Section)}"
                : "Unassigned";

            // 3. Populate Parent Phone String Link details
            var parents = await _studentParentRepo.GetAllWithIncludeAsync(sp => sp.Person, sp => sp.Person.Users);
            var primaryParent = parents.FirstOrDefault(sp => sp.StudentId == studentId);
            var parentUser = primaryParent?.Person?.Users?.FirstOrDefault();
            pageData.ParentPhoneNumber = parentUser?.PhoneNumber ?? "No Registered Contact Number";

            // 4. Hydrate Approved Marks List Feed
            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.Subject);
            var studentApprovedMarks = allMarks.Where(m => m.IsApproved && m.StudentRecordId == studentId).ToList();

            foreach (var mark in studentApprovedMarks)
            {
                pageData.MarksList.Add(new StudentDetailsMarkItemDto
                {
                    SubjectName = mark.Subject.SubjectName.ToUpper(), // Formats 'MATHEMATICS' or 'ARABIC' cleanly
                    AchievedScore = mark.MarkValue,
                    MaximumScore = mark.FullMark
                });
            }

            // 5. Calculate Cumulative GPA Percentage Display Value
            if (studentApprovedMarks.Any())
            {
                double averageGpa = studentApprovedMarks.Average(m => (double)(m.MarkValue / m.FullMark) * 100);
                pageData.TotalGPA = $"{Math.Round(averageGpa)}%";
            }
            else
            {
                pageData.TotalGPA = "0%";
            }

            // 6. Hydrate Attendance Calendar Heatmap Grid Matrix
            var allAttendance = await _studentAttendanceRepo.GetAllAsync();
            var targetedMonthlyAttendanceLogs = allAttendance
                .Where(sa => sa.StudentId == studentId &&
                             sa.AttendanceDate.Month == month &&
                             sa.AttendanceDate.Year == year)
                .ToList();

            foreach (var log in targetedMonthlyAttendanceLogs)
            {
                pageData.CalendarLogs.Add(new CalendarAttendanceDayDto
                {
                    DayNumber = log.AttendanceDate.Day,
                    StatusType = log.Status // Maps directly to 1, 2, 3, or 4 color-code states
                });
            }

            return pageData;
        }

        public async Task<IEnumerable<SupervisorTeacherSidebarDto>> GetSupervisedTeachersSidebarAsync(int supervisorPersonId, string searchTerm)
        {
            var sidebarList = new List<SupervisorTeacherSidebarDto>();

            // 1. Resolve the supervisor profile context
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return sidebarList;

            // 2. Locate Classrooms under this supervisor
            var allRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRoomIds = allRooms
                .Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId)
                .Select(cr => cr.ClassRoomId)
                .ToList();

            // 3. Find Teachers operating inside those specific Classrooms via Junction Table
            var classroomTeachers = await _classTeacherRepo.GetAllWithIncludeAsync(
                ct => ct.Teacher,
                ct => ct.Teacher.Person,
                ct => ct.Teacher.Person.Users,
                ct => ct.Subject
            );

            // Filter to retain teachers working within our supervised spaces
            var supervisedTeacherLinks = classroomTeachers
                .Where(ct => supervisedRoomIds.Contains(ct.ClassRoomId))
                .ToList();

            // Group the linkages by Teacher to prevent duplicate sidebar entity cards rows
            var uniqueTeachers = supervisedTeacherLinks
                .Select(ct => ct.Teacher)
                .GroupBy(t => t.TeacherId)
                .Select(g => g.First())
                .ToList();

            // Apply left sidebar text search criteria if active
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                uniqueTeachers = uniqueTeachers
                    .Where(t => t.Person.FirstName.ToLower().Contains(searchTerm) ||
                                t.Person.LastName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            // 4. Map Sidebar elements and cross-join display titles
            foreach (var teacher in uniqueTeachers)
            {
                var linksForThisTeacher = supervisedTeacherLinks.Where(ct => ct.TeacherId == teacher.TeacherId).ToList();

                // Format Classes string layout list (e.g., "SEVENTH / FIRST / THIRD")
                var classNames = linksForThisTeacher
                    .Select(ct => allRooms.FirstOrDefault(r => r.ClassRoomId == ct.ClassRoomId))
                    .Where(r => r != null)
                    .Select(r => $"{GetGradeWord(r.Grade.GradeNumber)} / {GetSectionWord(r.Section)}")
                    .Select(s => s.ToUpper())
                    .Distinct();

                string classesDisplayStr = string.Join(" / ", classNames);

                // Format Subjects string layout list (e.g., "SCIENCE - PHYSICS")
                var subjectNames = linksForThisTeacher
                    .Select(ct => ct.Subject.SubjectName.ToUpper())
                    .Distinct();

                string subjectsDisplayStr = string.Join(" - ", subjectNames);

                var associatedUser = teacher.Person.Users.FirstOrDefault();

                sidebarList.Add(new SupervisorTeacherSidebarDto
                {
                    TeacherID = teacher.TeacherId,
                    FullName = $"{teacher.Person.FirstName} {teacher.Person.LastName}".ToLower(), // Matches UI lowercase look
                    PhoneNumber = associatedUser?.PhoneNumber ?? "No Number",
                    ClassesDisplay = classesDisplayStr,
                    SubjectsDisplay = subjectsDisplayStr
                });
            }

            return sidebarList;
        }

        public async Task<TeacherDetailsPaneDto?> GetTeacherPaneDetailsAsync(int teacherId, int month, int year)
        {
            // 1. Fetch Teacher entity with core identity records graph
            var teachersList = await _teacherRepo.GetAllWithIncludeAndFilterAsync(
                t => t.TeacherId == teacherId,
                t => t.Person,
                t => t.Person.Users
            );
            var targetTeacher = teachersList.FirstOrDefault();
            if (targetTeacher == null) return null;

            var paneData = new TeacherDetailsPaneDto { TeacherID = teacherId };
            paneData.FullName = $"{targetTeacher.Person.FirstName} {targetTeacher.Person.LastName}".ToLower();

            var userAccount = targetTeacher.Person.Users.FirstOrDefault();
            paneData.PhoneNumber = userAccount?.PhoneNumber ?? "No Number";

            // 2. Extract layout text metadata fields utilizing our pre-designed link processing logic
            var allRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var allSchedules = await _scheduleRepo.GetAllAsync();
            var classroomTeachers = await _classTeacherRepo.GetAllWithIncludeAsync(ct => ct.Subject);

            var teacherWorkLinks = classroomTeachers.Where(ct => ct.TeacherId == teacherId).ToList();

            var classNames = teacherWorkLinks
                .Select(ct => allRooms.FirstOrDefault(r => r.ClassRoomId == ct.ClassRoomId))
                .Where(r => r != null)
                .Select(r => $"{GetGradeWord(r.Grade.GradeNumber)} / {GetSectionWord(r.Section)}".ToUpper())
                .Distinct();

            paneData.ClassesDisplay = string.Join(" / ", classNames);
            paneData.SubjectsDisplay = string.Join(" - ", teacherWorkLinks.Select(ct => ct.Subject.SubjectName.ToUpper()).Distinct());

            // 3. Resolve Weekly Work Schedule image attachment path
            // ScheduleType 2 = Teacher Personal Program Schedule, ReferenceID = TeacherID
            var personalSchedule = allSchedules.FirstOrDefault(s => s.ScheduleType == 2 && s.ReferenceId == teacherId);
            paneData.WeeklyWorkScheduleUrl = personalSchedule?.ImagePath ?? "uploads/schedules/default_teacher.png";

            // 4. Hydrate Monthly Teacher Attendance Grid Matrix (Color States Tracker)
            var allTeacherAttendance = await _teacherAttendanceRepo.GetAllAsync();
            var monthlyLogs = allTeacherAttendance
                .Where(ta => ta.TeacherId == teacherId && ta.AttendanceDate.Month == month && ta.AttendanceDate.Year == year)
                .ToList();

            foreach (var log in monthlyLogs)
            {
                paneData.AttendanceCalendar.Add(new CalendarAttendanceDayDto
                {
                    DayNumber = log.AttendanceDate.Day,
                    // Mapping Status: 1=Present (Green), 2=Absent (Red), 3=Late (Yellow), 4=Sick/Other (Black)
                    StatusType = log.Status
                });
            }

            return paneData;
        }

        // Private layout text normalization helpers
        private string GetGradeWord(int gradeNumber) => gradeNumber switch { 7 => "SEVENTH", 8 => "EIGHTH", 9 => "NINTH", _ => $"{gradeNumber}TH" };
        private string GetSectionWord(byte section) => section switch { 1 => "FIRST", 2 => "SECOND", 3 => "THIRD", _ => $"SEC {section}" };


    }
}
