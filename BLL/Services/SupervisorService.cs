using BLL.EntitiesDTOS.Supervisor;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using BLL.Notifications.Events;
using BLL.Notifications.Interfaces;
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
        IBaseRepositories<ChatRoom> _chatRoomRepo;
        IBaseRepositories<Message> _messageRepo;
        private readonly INotificationPublisher _notificationPublisher;

        public SupervisorService(IBaseRepositories<ClassRoom> classRoomRepo, IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<ClassroomStudent> classStudentRepo,
            IBaseRepositories<StudentAttendance> studentAttendanceRepo, IBaseRepositories<ToDoTask> taskRepo,
            IBaseRepositories<ClassroomTeacher> classTeacherRepo, IBaseRepositories<TeacherAttendance> teacherAttendanceRepo,
            IBaseRepositories<Announcement> announcementRepo,IBaseRepositories<AnnouncementClassroom> announcementClassroomRepo,
            IBaseRepositories<Mark> markRepo,IBaseRepositories<Schedule> scheduleRepo,
        IBaseRepositories<StudentRecord> studentRecordRepo,
        IBaseRepositories<ExamSchedule> examScheduleRepo, IBaseRepositories<StudentParent> studentParentRepo,
        IBaseRepositories<Teacher> teacherRepo, IBaseRepositories<ChatRoom> chatRoomRepo,
        IBaseRepositories<Message> messageRepo, INotificationPublisher notificationPublisher)
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
            _chatRoomRepo = chatRoomRepo;
            _messageRepo = messageRepo;
            _studentRecordRepo = studentRecordRepo;
            _notificationPublisher = notificationPublisher;

        }

        //public async Task<SupervisorMainDashboardDto> GetMainDashboardAsync(int supervisorPersonId)
        //{
        //    var dashboard = new SupervisorMainDashboardDto();
        //    var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        //    // 1. Get the current Supervisor's tracking primary key ID context
        //    var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
        //    var activeSupervisor = supervisors.FirstOrDefault();
        //    if (activeSupervisor == null) return dashboard;

        //    // 2. Load rooms assigned directly to this supervisor
        //    var rooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
        //    var supervisedRooms = rooms.Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId).ToList();
        //    var supervisedRoomIds = supervisedRooms.Select(cr => cr.ClassRoomId).ToList();

        //    dashboard.ClassesCount = supervisedRooms.Count;

        //    // Populate Dropdown collection mapping
        //    foreach (var r in supervisedRooms)
        //    {
        //        dashboard.SupervisedClasses.Add(new SupervisorClassDropdownDto
        //        {
        //            ClassRoomID = r.ClassRoomId,
        //            // Match the visual text casing strings in your screenshot layout ("SEVENTH - FIRST")
        //            ClassDisplayName = $"Grade {r.Grade.GradeNumber} - Section {r.Section}"
        //        });
        //    }

        //    // 3. Collect students operating inside those rooms
        //    var classroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.Student, cs => cs.Student.Person);
        //    var managedStudents = classroomStudents.Where(cs => supervisedRoomIds.Contains(cs.ClassRoomId)).ToList();
        //    var managedStudentIds = managedStudents.Select(cs => cs.StudentId).ToList();

        //    dashboard.TotalStudentsCount = managedStudentIds.Distinct().Count();

        //    // 4. Evaluate real-time Attendance logs for today
        //    var todayAttendance = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
        //        sa => sa.AttendanceDate == todayDate && managedStudentIds.Contains(sa.StudentId)
        //    );

        //    dashboard.AbsentTodayCount = todayAttendance.Count(sa => sa.Status == 2); // 2 = Absent
        //                                                                              // PresentCount calculation includes simple present metrics + late arrivals
        //    dashboard.PresentTodayCount = todayAttendance.Count(sa => sa.Status == 1 || sa.Status == 3);

        //    // 5. Hydrate the "Absent Today" Global Real-time Exception Grid List Feed View
        //    var alertRecords = todayAttendance.Where(sa => sa.Status == 2 || sa.Status == 3).ToList();
        //    foreach (var alert in alertRecords)
        //    {
        //        var studentInfo = managedStudents.FirstOrDefault(cs => cs.StudentId == alert.StudentId);
        //        if (studentInfo == null) continue;

        //        dashboard.ExceptionFeed.Add(new AbsentTodayGridItemDto
        //        {
        //            FullName = $"{studentInfo.Student.Person.FirstName} {studentInfo.Student.Person.LastName}",
        //            ClassName = $"Grade {studentInfo.ClassRoom.Grade.GradeNumber}",
        //            SectionName = $"Section {studentInfo.ClassRoom.Section}",
        //            Status = alert.Status == 2 ? "ABSENT" : "LATE"
        //        });
        //    }

        //    return dashboard;
        //}


        public async Task<SupervisorMainDashboardDto> GetMainDashboardAsync(int supervisorPersonId)
        {
            var dashboard = new SupervisorMainDashboardDto();

            var todayDate = DateOnly.FromDateTime(DateTime.Today);

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return dashboard;

            var rooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRooms = rooms.Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId).ToList();
            var supervisedRoomIds = supervisedRooms.Select(cr => cr.ClassRoomId).ToList();

            dashboard.ClassesCount = supervisedRooms.Count;

            foreach (var r in supervisedRooms)
            {
                dashboard.SupervisedClasses.Add(new SupervisorClassDropdownDto
                {
                    ClassRoomID = r.ClassRoomId,
                    ClassDisplayName = $"Grade {r.Grade.GradeNumber} - Section {r.Section}"
                });
            }

            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(
                cs => cs.Student,
                cs => cs.Student.Person,
                cs => cs.ClassRoom,
                cs => cs.ClassRoom.Grade
            );

            var managedStudents = classroomStudents.Where(cs => supervisedRoomIds.Contains(cs.ClassRoomId)).ToList();
            var managedStudentIds = managedStudents.Select(cs => cs.StudentId).ToList();

            dashboard.TotalStudentsCount = managedStudentIds.Distinct().Count();

            var todayAttendance = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                sa => sa.AttendanceDate == todayDate && managedStudentIds.Contains(sa.StudentId)
            );

            dashboard.AbsentTodayCount = todayAttendance.Count(sa => sa.Status == 2); // 2 = Absent
            dashboard.PresentTodayCount = todayAttendance.Count(sa => sa.Status == 1 || sa.Status == 3); // 1 = Present, 3 = Late

            var alertRecords = todayAttendance.Where(sa => sa.Status == 2 || sa.Status == 3).ToList();
            foreach (var alert in alertRecords)
            {
                var studentInfo = managedStudents.FirstOrDefault(cs => cs.StudentId == alert.StudentId);

                if (studentInfo == null || studentInfo.ClassRoom == null || studentInfo.ClassRoom.Grade == null)
                    continue;

                dashboard.ExceptionFeed.Add(new AbsentTodayGridItemDto
                {
                    FullName = $"{studentInfo.Student.Person.FirstName} {studentInfo.Student.Person.LastName}".Replace("  ", " ").Trim(),
                    ClassName = $"Grade {studentInfo.ClassRoom.Grade.GradeNumber}",
                    SectionName = $"Section {studentInfo.ClassRoom.Section}",
                    Status = alert.Status == 2 ? "ABSENT" : "LATE"
                });
            }

            return dashboard;
        }


        public async Task<ClassRollCallDto?> GetClassroomRollCallAsync(int supervisorPersonId, int classRoomId)
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return null;

            var targetRoom = await _classRoomRepo.GetByIdAsync(classRoomId);
            if (targetRoom == null || targetRoom.SupervisorId != activeSupervisor.SupervisorId) return null;

            var rollCall = new ClassRollCallDto();
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId,
                cs => cs.Student,
                cs => cs.Student.Person
            );

            var studentIdsInClass = classroomStudents.Select(cs => cs.StudentId).ToList();

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
                rollCall.StatusMessage = "Attendance was not taken yet";
            }

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
            var allTasks = await _taskRepo.GetAllWithIncludeAndFilterAsync(
                t => t.AssignedPersonID == supervisorPersonId && !t.IsDone,
                t => t.ClassRoom,
                t => t.ClassRoom.Grade
            );

            return allTasks.Select(t => new SupervisorTaskDto
            {
                TaskID = t.TaskID,
                TaskDescription = t.TaskDescription,
                IsDone = t.IsDone,
                DueDate = t.DueDate,
                ClassRoomID = t.ClassRoomID,
                PriorityLevel = t.PriorityLevel,

                ClassRoomName = t.ClassRoom != null && t.ClassRoom.Grade != null
                    ? $"{GetGradeOrdinalWord(t.ClassRoom.Grade.GradeNumber)}/{GetSectionOrdinalWord(t.ClassRoom.Section)}"
                    : "General Task"
            });
        }

        private static string GetGradeOrdinalWord(int gradeNumber)
        {
            return gradeNumber switch
            {
                1 => "First",
                2 => "Second",
                3 => "Third",
                4 => "Fourth",
                5 => "Fifth",
                6 => "Sixth",
                7 => "Seventh",
                8 => "Eighth",
                9 => "Ninth",
                10 => "Tenth",
                11 => "Eleventh",
                12 => "Twelfth",
                _ => $"Grade {gradeNumber}"
            };
        }

        private static string GetSectionOrdinalWord(byte sectionNumber)
        {
            return sectionNumber switch
            {
                1 => "First",
                2 => "Second",
                3 => "Third",
                4 => "Fourth",
                _ => $"Section {sectionNumber}"
            };
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
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null || await VerifyOversightAsync(activeSupervisor.SupervisorId, classRoomId) == false)
                return null;

            var sheet = new AttendanceSheetLoadDto();
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var classStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId, cs => cs.Student, cs => cs.Student.Person
            );

            var classTeachers = await _classTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.ClassRoomId == classRoomId, ct => ct.Teacher, ct => ct.Teacher.Person
            );

            var studentAttendanceToday = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                sa => sa.ClassRoomId == classRoomId && sa.AttendanceDate == todayDate
            );

            var teacherIdsInClass = classTeachers.Select(ct => ct.TeacherId).ToList();
            var teacherAttendanceToday = await _teacherAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                ta => ta.AttendanceDate == todayDate && teacherIdsInClass.Contains(ta.TeacherId)
            );

            sheet.IsAlreadyRecordedToday = studentAttendanceToday.Any() || teacherAttendanceToday.Any();

            foreach (var cs in classStudents)
            {
                var existingLog = studentAttendanceToday.FirstOrDefault(sa => sa.StudentId == cs.StudentId);
                sheet.Students.Add(new StudentAttendanceRowDto
                {
                    StudentID = cs.StudentId,
                    FullName = $"{cs.Student.Person.FirstName} {cs.Student.Person.LastName}",
                    Status = existingLog?.Status ?? 1, 
                    Note = existingLog?.Notes ?? string.Empty
                });
            }

            foreach (var ct in classTeachers)
            {
                var existingLog = teacherAttendanceToday.FirstOrDefault(ta => ta.TeacherId == ct.TeacherId);
                sheet.Teachers.Add(new TeacherAttendanceRowDto
                {
                    TeacherID = ct.TeacherId,
                    FullName = $"{ct.Teacher.Person.FirstName} {ct.Teacher.Person.LastName}",
                    Status = existingLog?.Status ?? 1, 
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

            var transaction = await _classRoomRepo.BeginTransactionAsync();
            try
            {
                var existingStudentLogs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    sa => sa.ClassRoomId == dto.ClassRoomID && sa.AttendanceDate == todayDate
                );

                foreach (var sDto in dto.StudentRecords)
                {
                    var matchedLog = existingStudentLogs.FirstOrDefault(sa => sa.StudentId == sDto.StudentID);
                    if (matchedLog != null)
                    {
                        matchedLog.Status = sDto.Status;
                        matchedLog.Notes = sDto.Note;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _studentAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
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

                var teacherIds = dto.TeacherRecords.Select(t => t.TeacherID).ToList();
                var existingTeacherLogs = await _teacherAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    ta => ta.AttendanceDate == todayDate && teacherIds.Contains(ta.TeacherId)
                );

                foreach (var tDto in dto.TeacherRecords)
                {
                    var matchedLog = existingTeacherLogs.FirstOrDefault(ta => ta.TeacherId == tDto.TeacherID);
                    if (matchedLog != null)
                    {
                        matchedLog.Status = tDto.Status;
                        matchedLog.Notes = tDto.Note;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _teacherAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
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

        private async Task<bool> VerifyOversightAsync(int supervisorId, int classRoomId)
        {
            var room = await _classRoomRepo.GetByIdAsync(classRoomId);
            return room != null && room.SupervisorId == supervisorId;
        }

        public async Task<AnnouncementManagementPageDto> LoadAnnouncementsPanelAsync(int senderPersonId)
        {
            var pageData = new AnnouncementManagementPageDto();

            pageData.TargetOptions.Add(new AnnouncementTargetDropdownDto { ClassRoomID = null, DisplayName = "All classes" });

            var classes = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            foreach (var cr in classes)
            {
                pageData.TargetOptions.Add(new AnnouncementTargetDropdownDto
                {
                    ClassRoomID = cr.ClassRoomId,
                    DisplayName = $"{cr.Grade.GradeNumber}th Grade / Class {cr.Section}" 
                });
            }

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
                var newAnnouncement = new Announcement
                {
                    SenderPersonId = senderPersonId,
                    Title = dto.Title,
                    AnnouncementBody = dto.Content,
                    IsGeneral = dto.IsGeneral,
                    CreatedAt = DateTime.UtcNow
                };
                await _announcementRepo.AddAsync(newAnnouncement);
                await _announcementRepo.SaveChangesAsync(); 

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

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return classCards;

            var allRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRooms = allRooms.Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId).ToList();
            var supervisedRoomIds = supervisedRooms.Select(cr => cr.ClassRoomId).ToList();

            var allClassroomStudents = await _classStudentRepo.GetAllAsync();
            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.ExamType);

            var allSchedules = await _scheduleRepo.GetAllAsync();
            var allExamSchedules = await _examScheduleRepo.GetAllAsync();

            foreach (var room in supervisedRooms)
            {
                var studentIdsInClass = allClassroomStudents
                    .Where(cs => cs.ClassRoomId == room.ClassRoomId)
                    .Select(cs => cs.StudentId)
                    .ToList();

                var classApprovedMarks = allMarks
                    .Where(m => m.IsApproved && studentIdsInClass.Contains(m.StudentRecordId))
                    .ToList();

                string averageDisplay = "N/A";
                if (classApprovedMarks.Any())
                {
                    double totalPercentageSum = classApprovedMarks
                        .Sum(m => (double)(m.MarkValue / m.FullMark) * 100);

                    double average = totalPercentageSum / classApprovedMarks.Count;
                    averageDisplay = $"{Math.Round(average)} %";
                }

                var weeklyProg = allSchedules.FirstOrDefault(s => s.ScheduleType == 1 && s.ReferenceId == room.ClassRoomId);

                var examProg = allExamSchedules.FirstOrDefault(es => es.GradeId == room.GradeId && es.AcademicYear == currentYear);

                classCards.Add(new SupervisorClassCardDto
                {
                    ClassRoomID = room.ClassRoomId,
                    ClassName = $"{room.Grade.GradeNumber}th grade / {GetSectionNameWord(room.Section)}",
                    NumberOfStudents = studentIdsInClass.Count,
                    ClassAverage = averageDisplay, 
                    WeeklyWorkScheduleUrl = weeklyProg?.ImagePath ?? "uploads/schedules/default_schedule.png",
                    SemesterExamScheduleUrl = examProg?.ImagePath ?? "uploads/schedules/default_exams.png"
                });
            }

            return classCards;
        }

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

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return studentList;

            var rooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRoomIds = rooms
                .Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId)
                .Select(cr => cr.ClassRoomId)
                .ToList();

            if (classRoomId.HasValue && supervisedRoomIds.Contains(classRoomId.Value))
            {
                supervisedRoomIds = new List<int> { classRoomId.Value };
            }

            var classroomStudents = await _classStudentRepo.GetAllWithIncludeAsync(
                cs => cs.Student,
                cs => cs.Student.Person
            );

            var filteredClassStudents = classroomStudents
                .Where(cs => supervisedRoomIds.Contains(cs.ClassRoomId))
                .ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                filteredClassStudents = filteredClassStudents
                    .Where(cs => cs.Student.Person.FirstName.ToLower().Contains(searchTerm) ||
                                 cs.Student.Person.LastName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            var managedStudentIds = filteredClassStudents.Select(cs => cs.StudentId).ToList();

            var allMarks = await _markRepo.GetAllAsync();
            var allAttendance = await _studentAttendanceRepo.GetAllAsync();

            foreach (var cs in filteredClassStudents)
            {
                var studentMarks = allMarks.Where(m => m.IsApproved && m.StudentRecordId == cs.StudentId).ToList();
                string gpaDisplay = "0 %";
                if (studentMarks.Any())
                {
                    double average = studentMarks.Average(m => (double)(m.MarkValue / m.FullMark) * 100);
                    gpaDisplay = $"{Math.Round(average)} %";
                }

                var studentHistory = allAttendance.Where(sa => sa.StudentId == cs.StudentId).ToList();
                string attendanceDisplay = "100 %";
                if (studentHistory.Any())
                {
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
                    GPA = gpaDisplay,             
                    AttendanceRate = attendanceDisplay 
                });
            }

            return studentList;
        }

        public async Task<bool> SaveStudentAttendanceWorkflowAsync(int supervisorPersonId, SaveStudentAttendanceDto dto)
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();

            if (activeSupervisor == null || await VerifyOversightAsync(activeSupervisor.SupervisorId, dto.ClassRoomID) == false)
                return false;

            var todayDate = DateOnly.FromDateTime(DateTime.Today);
            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                var existingStudentLogs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    sa => sa.ClassRoomId == dto.ClassRoomID && sa.AttendanceDate == todayDate
                );

                foreach (var sDto in dto.StudentRecords)
                {
                    var matchedLog = existingStudentLogs.FirstOrDefault(sa => sa.StudentId == sDto.StudentID);
                    if (matchedLog != null)
                    {
                        matchedLog.Status = sDto.Status;
                        matchedLog.Notes = sDto.Note;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _studentAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
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
                await _classRoomRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> SaveTeacherAttendanceWorkflowAsync(int supervisorPersonId, SaveTeacherAttendanceDto dto)
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return false;

            var todayDate = DateOnly.FromDateTime(DateTime.Today); 
            var transaction = await _classRoomRepo.BeginTransactionAsync();

            try
            {
                var teacherIds = dto.TeacherRecords.Select(t => t.TeacherID).ToList();
                var existingTeacherLogs = await _teacherAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    ta => ta.AttendanceDate == todayDate && teacherIds.Contains(ta.TeacherId)
                );

                foreach (var tDto in dto.TeacherRecords)
                {
                    var matchedLog = existingTeacherLogs.FirstOrDefault(ta => ta.TeacherId == tDto.TeacherID);

                    var missedPeriods = tDto.Status == 1 ? null : tDto.MissedPeriodsCount;

                    if (matchedLog != null)
                    {
                        matchedLog.Status = tDto.Status;
                        matchedLog.MissedPeriodsCount = missedPeriods;
                        matchedLog.UpdatedAt = DateTime.UtcNow;
                        _teacherAttendanceRepo.UpdateAsync(matchedLog);
                    }
                    else
                    {
                        var newLog = new TeacherAttendance
                        {
                            TeacherId = tDto.TeacherID,
                            AttendanceDate = todayDate,
                            Status = tDto.Status,
                            MissedPeriodsCount = missedPeriods,
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


        public async Task<StudentDetailsPageDto?> GetStudentDetailedProfileAsync(int studentId, int month, int year)
        {
            var studentRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(
             sr => sr.StudentId == studentId,
             sr => sr.Student,
             sr => sr.Student.Person
                );

            
            var targetRecord = studentRecords.FirstOrDefault();
            if (targetRecord == null) return null;

            var pageData = new StudentDetailsPageDto { StudentID = studentId };
            pageData.FullName = $"{targetRecord.Student.Person.FirstName} {targetRecord.Student.Person.LastName}";

            var classLinks = await _classStudentRepo.GetAllWithIncludeAsync(cs => cs.ClassRoom, cs => cs.ClassRoom.Grade);
            var activeLink = classLinks.FirstOrDefault(cs => cs.StudentId == studentId);
            pageData.ClassAndSection = activeLink != null
                ? $"{activeLink.ClassRoom.Grade.GradeNumber}th / {GetSectionNameWord(activeLink.ClassRoom.Section)}"
                : "Unassigned";

            var parents = await _studentParentRepo.GetAllWithIncludeAsync(
    sp => sp.Parent,
    sp => sp.Parent.Person,
    sp => sp.Parent.Person.Users
);

            var primaryParentLink = parents.FirstOrDefault(sp => sp.StudentId == studentId);

            var parentUser = primaryParentLink?.Parent?.Person?.Users?.FirstOrDefault();

            pageData.ParentPhoneNumber = parentUser?.PhoneNumber ?? "No Registered Contact Number";

            var allMarks = await _markRepo.GetAllWithIncludeAsync(m => m.Subject);
            var studentApprovedMarks = allMarks.Where(m => m.IsApproved && m.StudentRecordId == studentId).ToList();

            foreach (var mark in studentApprovedMarks)
            {
                pageData.MarksList.Add(new StudentDetailsMarkItemDto
                {
                    SubjectName = mark.Subject.SubjectName.ToUpper(), 
                    AchievedScore = mark.MarkValue,
                    MaximumScore = mark.FullMark
                });
            }

            if (studentApprovedMarks.Any())
            {
                double averageGpa = studentApprovedMarks.Average(m => (double)(m.MarkValue / m.FullMark) * 100);
                pageData.TotalGPA = $"{Math.Round(averageGpa)}%";
            }
            else
            {
                pageData.TotalGPA = "0%";
            }

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
                    StatusType = log.Status
                });
            }

            return pageData;
        }

        public async Task<IEnumerable<SupervisorTeacherSidebarDto>> GetSupervisedTeachersSidebarAsync(int supervisorPersonId, string searchTerm)
        {
            var sidebarList = new List<SupervisorTeacherSidebarDto>();

            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
            var activeSupervisor = supervisors.FirstOrDefault();
            if (activeSupervisor == null) return sidebarList;

            var allRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            var supervisedRoomIds = allRooms
                .Where(cr => cr.SupervisorId == activeSupervisor.SupervisorId)
                .Select(cr => cr.ClassRoomId)
                .ToList();

            var classroomTeachers = await _classTeacherRepo.GetAllWithIncludeAsync(
                ct => ct.Teacher,
                ct => ct.Teacher.Person,
                ct => ct.Teacher.Person.Users,
                ct => ct.Subject
            );

            var supervisedTeacherLinks = classroomTeachers
                .Where(ct => supervisedRoomIds.Contains(ct.ClassRoomId))
                .ToList();

            var uniqueTeachers = supervisedTeacherLinks
                .Select(ct => ct.Teacher)
                .GroupBy(t => t.TeacherId)
                .Select(g => g.First())
                .ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                uniqueTeachers = uniqueTeachers
                    .Where(t => t.Person.FirstName.ToLower().Contains(searchTerm) ||
                                t.Person.LastName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            foreach (var teacher in uniqueTeachers)
            {
                var linksForThisTeacher = supervisedTeacherLinks.Where(ct => ct.TeacherId == teacher.TeacherId).ToList();

                var classNames = linksForThisTeacher
                    .Select(ct => allRooms.FirstOrDefault(r => r.ClassRoomId == ct.ClassRoomId))
                    .Where(r => r != null)
                    .Select(r => $"{GetGradeWord(r.Grade.GradeNumber)} / {GetSectionWord(r.Section)}")
                    .Select(s => s.ToUpper())
                    .Distinct();

                string classesDisplayStr = string.Join(" / ", classNames);

                var subjectNames = linksForThisTeacher
                    .Select(ct => ct.Subject.SubjectName.ToUpper())
                    .Distinct();

                string subjectsDisplayStr = string.Join(" - ", subjectNames);

                var associatedUser = teacher.Person.Users.FirstOrDefault();

                sidebarList.Add(new SupervisorTeacherSidebarDto
                {
                    TeacherID = teacher.TeacherId,
                    FullName = $"{teacher.Person.FirstName} {teacher.Person.LastName}".ToLower(), 
                    PhoneNumber = associatedUser?.PhoneNumber ?? "No Number",
                    ClassesDisplay = classesDisplayStr,
                    SubjectsDisplay = subjectsDisplayStr
                });
            }

            return sidebarList;
        }

        public async Task<TeacherDetailsPaneDto?> GetTeacherPaneDetailsAsync(int teacherId, int month, int year)
        {
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

            var personalSchedule = allSchedules.FirstOrDefault(s => s.ScheduleType == 2 && s.ReferenceId == teacherId);
            paneData.WeeklyWorkScheduleUrl = personalSchedule?.ImagePath ?? "uploads/schedules/default_teacher.png";

            var allTeacherAttendance = await _teacherAttendanceRepo.GetAllAsync();
            var monthlyLogs = allTeacherAttendance
                .Where(ta => ta.TeacherId == teacherId && ta.AttendanceDate.Month == month && ta.AttendanceDate.Year == year)
                .ToList();

            foreach (var log in monthlyLogs)
            {
                paneData.AttendanceCalendar.Add(new CalendarAttendanceDayDto
                {
                    DayNumber = log.AttendanceDate.Day,
                    StatusType = log.Status
                });
            }

            return paneData;
        }

        private string GetGradeWord(int gradeNumber) => gradeNumber switch { 7 => "SEVENTH", 8 => "EIGHTH", 9 => "NINTH", _ => $"{gradeNumber}TH" };
        private string GetSectionWord(byte section) => section switch { 1 => "FIRST", 2 => "SECOND", 3 => "THIRD", _ => $"SEC {section}" };


        public async Task<IEnumerable<ChatThreadDto>> GetSupervisorChatThreadsAsync(int supervisorPersonId)
        {

            try
            {
                var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(s => s.PersonId == supervisorPersonId);
                var activeSupervisor = supervisors.FirstOrDefault();
                if (activeSupervisor != null)
                {
                    var supervisedRooms = await _classRoomRepo.GetAllWithIncludeAndFilterAsync(cr => cr.SupervisorId == activeSupervisor.SupervisorId);
                    var roomIds = supervisedRooms.Select(r => r.ClassRoomId).ToList();

                    if (roomIds.Any())
                    {
                        var classStudents = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(cs => roomIds.Contains(cs.ClassRoomId));
                        var studentIds = classStudents.Select(cs => cs.StudentId).Distinct().ToList();

                        if (studentIds.Any())
                        {
                            var studentParents = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                                sp => studentIds.Contains(sp.StudentId),
                                sp => sp.Parent
                            );

                            foreach (var sp in studentParents)
                            {
                                int parentPersonId = sp.Parent != null ? sp.Parent.PersonId : 0;
                                if (parentPersonId > 0)
                                {
                                    var existing = await _chatRoomRepo.GetAllWithIncludeAndFilterAsync(
                                        cr => cr.StudentFocusId == sp.StudentId &&
                                              cr.SupervisorPersonId == supervisorPersonId &&
                                              cr.ParentPersonId == parentPersonId
                                    );

                                    var room = existing.FirstOrDefault();
                                    if (room == null)
                                    {
                                        var newRoom = new ChatRoom
                                        {
                                            StudentFocusId = sp.StudentId,
                                            SupervisorPersonId = supervisorPersonId,
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
            }
            catch
            {
            }


            var activeRooms = await _chatRoomRepo.GetAllWithIncludeAndFilterAsync(
                cr => cr.SupervisorPersonId == supervisorPersonId && cr.IsActive,
                cr => cr.StudentFocus,       
                cr => cr.StudentFocus.Person,
                cr => cr.ParentPerson        
            );

            var threadsList = new List<ChatThreadDto>();

            foreach (var room in activeRooms)
            {
                threadsList.Add(new ChatThreadDto
                {
                    ChatRoomID = room.ChatRoomId,
                    ParentPersonID = room.ParentPersonId,
                    ParentName = $"{room.ParentPerson.FirstName} {room.ParentPerson.LastName}".Trim(),
                    StudentName = $"{room.StudentFocus.Person.FirstName} {room.StudentFocus.Person.LastName}".Trim(),
                    LastMessage = room.LastMessageContent ?? "No messages exchanged yet...",
                    LastMessageTime = room.LastMessageAt
                });
            }

            return threadsList.OrderByDescending(t => t.LastMessageTime ?? DateTime.MinValue);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int supervisorPersonId, int chatRoomId)
        {
            var rawMessages = await _messageRepo.GetAllWithIncludeAndFilterAsync(
                m => m.ChatRoomId == chatRoomId
            );

            var unreadMessages = rawMessages.Where(m => m.SenderPersonId != supervisorPersonId && m.ReadAt == null).ToList();
            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.ReadAt = DateTime.UtcNow;
                    _messageRepo.UpdateAsync(msg);
                }
                await _messageRepo.SaveChangesAsync();
            }

            return rawMessages
                .OrderBy(m => m.SentAt) 
                .Select(m => new ChatMessageDto
                {
                    MessageID = m.MessageId,
                    SenderPersonID = m.SenderPersonId,
                    MessageContent = m.MessageContent,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsMe = m.SenderPersonId == supervisorPersonId
                });
        }

        public async Task<bool> SendMessageAsync(int senderPersonId, SendMessageDto dto)
        {
            var room = await _chatRoomRepo.GetByIdAsync(dto.ChatRoomID);
            if (room == null || !room.IsActive) return false;

            if (room.SupervisorPersonId != senderPersonId && room.ParentPersonId != senderPersonId)
                return false;

            var transaction = await _chatRoomRepo.BeginTransactionAsync();
            try
            {
                var timestamp = DateTime.UtcNow;

                var newMessage = new Message
                {
                    ChatRoomId = dto.ChatRoomID,
                    SenderPersonId = senderPersonId,
                    MessageContent = dto.MessageContent,
                    SentAt = timestamp,
                    ReadAt = null 
                };
                await _messageRepo.AddAsync(newMessage);
                await _messageRepo.SaveChangesAsync();


                room.LastMessageContent = dto.MessageContent.Length > 255
                    ? dto.MessageContent.Substring(0, 252) + "..."
                    : dto.MessageContent;

                room.LastMessageAt = timestamp;
                _chatRoomRepo.UpdateAsync(room);
                await _chatRoomRepo.SaveChangesAsync();

                await _chatRoomRepo.CommitTransactionAsync();

                int receiverPersonId = (senderPersonId == room.SupervisorPersonId)
                    ? room.ParentPersonId
                    : room.SupervisorPersonId;

                await _notificationPublisher.PublishAsync(new ChatMessageSentEvent
                {
                    ChatRoomId = room.ChatRoomId,
                    SenderPersonId = senderPersonId,
                    ReceiverPersonId = receiverPersonId,
                    StudentFocusId = room.StudentFocusId,
                    MessageContent = newMessage.MessageContent,
                    MessageId = newMessage.MessageId,
                    OccurredAt = timestamp
                });



                return true;
            }
            catch
            {
                await _classRoomRepo.RollbackTransactionAsync();
                return false;
            }
        }



    }



}
