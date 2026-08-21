using BLL.EntitiesDTOS.Teacher;
using DAL.Entities;
using DAL.Interfaces;
using BLL.Interfaces;
using BLL.Notifications.Events;
using BLL.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;
        private readonly IBaseRepositories<ClassroomTeacher> _classroomTeacherRepo;
        private readonly IBaseRepositories<AnnouncementClassroom> _announcementClassroomRepo;
        private readonly IBaseRepositories<Announcement> _announcementRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classroomStudentRepo;
        private readonly IBaseRepositories<Mark> _markRepo;
        private readonly IBaseRepositories<StudentRecord> _studentRecordRepo;
        private readonly IBaseRepositories<StudentAttendance> _studentAttendanceRepo;
        private readonly IBaseRepositories<StudentNote> _studentNoteRepo;
        private readonly IBaseRepositories<DailyLesson> _dailyLessonRepo;
        private readonly IBaseRepositories<Homework> _homeworkRepo;
        private readonly IBaseRepositories<Schedule> _scheduleRepo;
        private readonly IBaseRepositories<ExamSchedule> _examScheduleRepo;
        private readonly INotificationPublisher _notificationPublisher;


        public TeacherService(
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<Teacher> teacherRepo,
            IBaseRepositories<ClassroomTeacher> classroomTeacherRepo,
            IBaseRepositories<AnnouncementClassroom> announcementClassroomRepo,
            IBaseRepositories<Announcement> announcementRepo,
            IBaseRepositories<User> userRepo,
            IBaseRepositories<ClassroomStudent> classroomStudentRepo, IBaseRepositories<Mark> markRepo,
            IBaseRepositories<StudentRecord> studentRecordRepo, IBaseRepositories<StudentAttendance> studentAttendanceRepo,
            IBaseRepositories<StudentNote> studentNoteRepo, IBaseRepositories<DailyLesson> dailyLessonRepo,
            IBaseRepositories<Homework> homeworkRepo, INotificationPublisher notificationPublisher,
            IBaseRepositories<Schedule> scheduleRepo,
            IBaseRepositories<ExamSchedule> examScheduleRepo)
        {                                                            
            _personRepo = personRepo;
            _teacherRepo = teacherRepo;
            _classroomTeacherRepo = classroomTeacherRepo;
            _announcementClassroomRepo = announcementClassroomRepo;
            _announcementRepo = announcementRepo;
            _userRepo = userRepo;
            _classroomStudentRepo = classroomStudentRepo;
            _markRepo = markRepo;
            _studentRecordRepo = studentRecordRepo;
            _studentAttendanceRepo = studentAttendanceRepo;
            _studentNoteRepo = studentNoteRepo;
            _dailyLessonRepo = dailyLessonRepo;
            _homeworkRepo = homeworkRepo;
            _notificationPublisher = notificationPublisher;
            _scheduleRepo = scheduleRepo;
            _examScheduleRepo = examScheduleRepo;
        }

        public async Task<TeacherDashboardDto?> GetTeacherDashboardAsync(int teacherPersonId)
        {
            

            var persons = await _personRepo.GetAllWithIncludeAndFilterAsync(p => p.PersonId == teacherPersonId);
            var person = persons.FirstOrDefault();
            if (person == null) return null;

            var dashboard = new TeacherDashboardDto
            {
                TeacherName = $"{person.FirstName} {person.LastName}".ToUpper()
            };

            
            var teachers = await _teacherRepo.GetAllWithIncludeAndFilterAsync(t => t.PersonId == teacherPersonId);
            var teacher = teachers.FirstOrDefault();
            if (teacher == null)
                return dashboard; 



            var taughtClasses = await _classroomTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacher.TeacherId
            );
            var assignedClassroomIds = taughtClasses.Select(ct => ct.ClassRoomId).Distinct().ToList();



            var targetedClassJunctions = await _announcementClassroomRepo.GetAllWithIncludeAndFilterAsync(
                ac => assignedClassroomIds.Contains(ac.ClassRoomId)
            );
            var targetedAnnouncementIds = targetedClassJunctions.Select(ac => ac.AnnouncementId).Distinct().ToList();



            var visibleAnnouncements = await _announcementRepo.GetAllWithIncludeAndFilterAsync(
                a => a.IsGeneral == true || targetedAnnouncementIds.Contains(a.AnnouncementId),
                a => a.SenderPerson
            );

            dashboard.Announcements = visibleAnnouncements.Select(a => new TeacherAnnouncementDto
            {
                AnnouncementID = a.AnnouncementId,
                Title = a.Title,
                Body = a.AnnouncementBody,
                CreatedAt = a.CreatedAt,
                IsGeneral = a.IsGeneral,
                SenderName = a.SenderPerson != null ? $"{a.SenderPerson.FirstName} {a.SenderPerson.LastName}".ToUpper() : "ADMINISTRATION"
            }).OrderByDescending(a => a.CreatedAt).ToList();

            return dashboard;
        }




        public async Task<TeacherDetailedProfileDto?> GetTeacherDetailedProfileAsync(int teacherPersonId)
        {


            var persons = await _personRepo.GetAllWithIncludeAndFilterAsync(p => p.PersonId == teacherPersonId);
            var person = persons.FirstOrDefault();
            if (person == null)
                return null;


            var users = await _userRepo.GetAllWithIncludeAndFilterAsync(u => u.PersonId == teacherPersonId);
            var user = users.FirstOrDefault();
            if (user == null) 
                return null;


            var teachers = await _teacherRepo.GetAllWithIncludeAndFilterAsync(t => t.PersonId == teacherPersonId);
            var teacher = teachers.FirstOrDefault();
            if (teacher == null) return null;

            var profileDto = new TeacherDetailedProfileDto
            {
                TeacherID = teacher.TeacherId,
                FullName = $"{person.FirstName} {person.SecondName} {person.LastName}".ToUpper(),
                PhoneNumber = user.PhoneNumber,
                Email = !string.IsNullOrEmpty(user.Email) ? user.Email : null
            };

            
            var schedules = await _classroomTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacher.TeacherId,
                ct => ct.ClassRoom,
                ct => ct.ClassRoom.Grade,
                ct => ct.Subject
            );

            
            var SchedulesList = new List<string>();
            var SubjectsList = new List<string>();

          

            foreach (var item in schedules)
            {
                if (item.ClassRoom != null && item.Subject != null)
                {
                    int classGradeNumber = item.ClassRoom.Grade?.GradeNumber ?? 0;
                    byte sectionNumber = item.ClassRoom.Section;
                    string subjectName = item.Subject.SubjectName;

                    string classGradeNumberString = GetGradeNameWord(classGradeNumber);
                    string sectionNumberString = GetSectionNameWord(sectionNumber);

                    //هي مشان الفورمات

                    string formattedRow = $"{classGradeNumberString}/{sectionNumberString}/{subjectName}";
                    SchedulesList.Add(formattedRow);

                    SubjectsList.Add(subjectName);
                }
            }

            
            profileDto.Schedules = SchedulesList.Distinct().ToList();
            profileDto.SubjectsTaught = SubjectsList.Distinct().ToList();

            return profileDto;
        }


        public async Task<IEnumerable<TeacherClassChipDto>> GetTeacherClassesChipsAsync(int teacherPersonId)
        {


            var teachers = await _teacherRepo.GetAllWithIncludeAndFilterAsync(t => t.PersonId == teacherPersonId);
            var teacher = teachers.FirstOrDefault();
            if (teacher == null) return Enumerable.Empty<TeacherClassChipDto>();

            var schedules = await _classroomTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacher.TeacherId,
                ct => ct.ClassRoom,
                ct => ct.ClassRoom.Grade,
                ct => ct.Subject
            );

            return schedules.Select(item => new TeacherClassChipDto
            {
                ClassRoomID = item.ClassRoomId,
                SubjectID = item.SubjectId,
                DisplayText = $"{GetGradeNameWord(item.ClassRoom?.Grade?.GradeNumber ?? 0)}/{GetSectionNameWord(item.ClassRoom?.Section ?? 0)}/{item.Subject?.SubjectName}".ToUpper()
            }).DistinctBy(c => new { c.ClassRoomID, c.SubjectID }).ToList();
        }

        public async Task<IEnumerable<ClassStudentListDto>> GetStudentsInClassAsync(int classRoomId)
        {
            var enrollments = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId,
                cs => cs.Student,
                cs => cs.Student.Person
            );

            return enrollments.Select(e => new ClassStudentListDto
            {
                StudentID = e.StudentId,
                FullName = $"{e.Student.Person.FirstName} {e.Student.Person.SecondName} {e.Student.Person.LastName}".ToUpper()
            }).OrderBy(s => s.FullName).ToList();
        }


        private string GetGradeNameWord(int? gradeNumber)
        {
            return gradeNumber switch
            {
                1 => "FIRST",
                2 => "SECOND",
                3 => "THIRD",
                4 => "FOURTH",
                5 => "FIFTH",
                6 => "SIXTH",
                7 => "SEVENTH",
                8 => "EIGHTH",
                9 => "NINTH",
                10 => "TENTH",
                11 => "ELEVENTH",
                12 => "TWELFTH",
                _ => $"GRADE {gradeNumber ?? 0}"
            };
        }

        private string GetSectionNameWord(byte? sectionNumber)
        {
            return sectionNumber switch
            {
                1 => "FIRST",
                2 => "SECOND",
                3 => "THIRD",
                4 => "FOURTH",
                5 => "FIFTH",
                6 => "SIXTH",
                7 => "SEVENTH",
                8 => "EIGHTH",
                9 => "NINTH",
                10 => "TENTH",
                _ => $"SECTION {sectionNumber ?? 0}"
            };
        }


        public async Task<bool> SaveStudentGradesAsync(SaveGradesBulkDto inputDto)
        {
            if (inputDto == null || !inputDto.StudentMarks.Any())
                return false;

            short currentAcademicYear = (short)DateTime.UtcNow.Year;

            var studentIds = inputDto.StudentMarks.Select(sm => sm.StudentID).ToList();
            var allPossibleRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(
                sr => studentIds.Contains(sr.StudentId)
            );

            foreach (var studentMark in inputDto.StudentMarks)
            {
                var activeRecord = allPossibleRecords
                    .Where(sr => sr.StudentId == studentMark.StudentID && sr.StudyYear == currentAcademicYear)
                    .FirstOrDefault();

                if (activeRecord == null)
                {
                    activeRecord = allPossibleRecords
                        .Where(sr => sr.StudentId == studentMark.StudentID)
                        .OrderByDescending(sr => sr.StudyYear)
                        .FirstOrDefault();
                }

                if (activeRecord == null) continue;

                var newMark = new Mark
                {
                    StudentRecordId = activeRecord.StudentRecordId,
                    SubjectId = inputDto.SubjectID,
                    ExamTypeId = inputDto.ExamTypeID,
                    MarkValue = studentMark.MarkValue,
                    FullMark = inputDto.FullMark,
                    ExamDate = inputDto.ExamDate,
                    Notes = studentMark.Notes,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _markRepo.AddAsync(newMark);
            }

            await _markRepo.SaveChangesAsync();

            var targetStudentIds = inputDto.StudentMarks.Select(m => m.StudentID).Distinct().ToList();
            await _notificationPublisher.PublishAsync(new MarksReleasedEvent
            {
                //ClassRoomId = inputDto.ClassRoomID,
                SubjectId = inputDto.SubjectID,
                ExamTypeId = inputDto.ExamTypeID,
                ExamDate = inputDto.ExamDate,
                StudentIds = targetStudentIds
            });


            return true;
        }

        public async Task<bool> SaveBulkAttendanceAsync(SaveAttendanceBulkDto inputDto)
        {
            if (inputDto == null || !inputDto.StudentAttendances.Any())
                return false;

            //DateTime parsedTargetDate = inputDto.AttendanceDate.ToDateTime(TimeOnly.MinValue);

            foreach (var input in inputDto.StudentAttendances)
            {
                var existingLogs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                    a => a.StudentId == input.StudentID && a.AttendanceDate == inputDto.AttendanceDate
                );
                var existingLog = existingLogs.FirstOrDefault();

                if (existingLog != null)
                {
                    existingLog.Status = input.Status;
                    existingLog.Notes = input.Notes;
                    existingLog.UpdatedAt = DateTime.UtcNow;
                    _studentAttendanceRepo.UpdateAsync(existingLog);
                }
                else
                {
                    var newAttendance = new StudentAttendance
                    {
                        StudentId = input.StudentID,
                        ClassRoomId = inputDto.ClassRoomID,
                        AttendanceDate = inputDto.AttendanceDate,
                        Status = input.Status,
                        Notes = input.Notes,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _studentAttendanceRepo.AddAsync(newAttendance);
                }
            }

            await _studentAttendanceRepo.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<ClassStudentListDto>> GetStudentsInClassWithSearchAsync(int classRoomId, string? searchName)
        {

            var enrollments = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.ClassRoomId == classRoomId,
                cs => cs.Student,
                cs => cs.Student.Person
            );

            var query = enrollments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanedSearch = searchName.Trim().ToUpper();

                query = query.Where(e =>
                    (e.Student.Person.FirstName.ToUpper() + " " +
             e.Student.Person.SecondName.ToUpper() + " " +
             e.Student.Person.LastName.ToUpper()).Contains(cleanedSearch) ||

            (e.Student.Person.FirstName.ToUpper() + " " +
             e.Student.Person.LastName.ToUpper()).Contains(cleanedSearch)
                );
            }

            return query.Select(e => new ClassStudentListDto
            {
                StudentID = e.StudentId,
                FullName = $"{e.Student.Person.FirstName} {e.Student.Person.SecondName} {e.Student.Person.LastName}".ToUpper()
            }).OrderBy(s => s.FullName).ToList();
        }

        public async Task<bool> SaveStudentNoteAsync(int teacherPersonId, SaveStudentNoteDto noteDto)
        {
            if (noteDto == null || string.IsNullOrWhiteSpace(noteDto.NoteContent))
                return false;

            var newNote = new StudentNote
            {
                StudentId = noteDto.StudentID,
                TeacherPersonId = teacherPersonId, 
                NoteContent = noteDto.NoteContent,
                CreatedAt = noteDto.CreatedAt ?? DateTime.UtcNow
            };


            await _studentNoteRepo.AddAsync(newNote);
            await _studentNoteRepo.SaveChangesAsync();

            await _notificationPublisher.PublishAsync(new StudentNoteAddedEvent
            {
                StudentId = newNote.StudentId,
                TeacherPersonId = teacherPersonId,
                NoteContent = newNote.NoteContent,
                NoteId = newNote.NoteId,
                OccurredAt = newNote.CreatedAt ?? DateTime.UtcNow
            });


            return true;
        }


        public async Task<bool> SaveDailyLessonAsync(int teacherPersonId, SaveDailyLessonDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Explanation))
                return false;

            var targetDate = dto.LessonDate;

            var existingLessons = await _dailyLessonRepo.GetAllWithIncludeAndFilterAsync(
                dl => dl.ClassRoomID == dto.ClassRoomID &&
                      dl.SubjectID == dto.SubjectID &&
                      dl.LessonDate == targetDate
            );

            var existingLesson = existingLessons.FirstOrDefault();

            if (existingLesson != null)
            {
                existingLesson.Description = dto.Explanation;
                existingLesson.CreatedAt = DateTime.UtcNow; 
                _dailyLessonRepo.UpdateAsync(existingLesson);
            }
            else
            {
                var newLesson = new DailyLesson
                {
                    ClassRoomID = dto.ClassRoomID,
                    SubjectID = dto.SubjectID,
                    TeacherPersonID = teacherPersonId,
                    LessonDate = targetDate,
                    Description = dto.Explanation,
                    CreatedAt = DateTime.UtcNow
                };

                await _dailyLessonRepo.AddAsync(newLesson);
            }

            await _dailyLessonRepo.SaveChangesAsync();
            return true;
        }



        public async Task<bool> CreateHomeworkAssignmentAsync(int teacherPersonId, SaveHomeworkDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return false;

            string? savedAttachmentPath = null;

            // 💡 معالجة رفع الملف المرفق محلياً بالـ GUID والمسار المادي للمشروع في حال وجوده
            if (dto.AttachmentFile != null && dto.AttachmentFile.Length > 0)
            {
                string fileExtension = Path.GetExtension(dto.AttachmentFile.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // تثبيت ممر المجلد المحلي بداخل wwwroot/uploads/homeworks
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "homeworks");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string physicalFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                // حفظ ملف الواجب الفعلي على القرص الصلب محلياً
                using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
                {
                    await dto.AttachmentFile.CopyToAsync(fileStream);
                }

                // استخلاص المسار النسبي الآمن للتخزين بجدول قاعدة البيانات
                savedAttachmentPath = $"uploads/homeworks/{uniqueFileName}";
            }

            // بناء كائن الواجب المنزلي الجديد مع الالتزام التام بمسميات حقول السكافولدينج الكبيرة لديك
            var newHomework = new Homework
            {
                ClassRoomId = dto.ClassRoomID,
                SubjectId = dto.SubjectID,
                TeacherPersonId = teacherPersonId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                AttachmentPath = savedAttachmentPath, // قد يكون مساراً نصياً أو NULL بسلام
                CreatedAt = DateTime.UtcNow
            };

            await _homeworkRepo.AddAsync(newHomework);
            await _homeworkRepo.SaveChangesAsync(); // توليد الـ HomeworkID تلقائياً

            // نشر الإشعار للأبناء والطلاب المسجلين
            await _notificationPublisher.PublishAsync(new HomeworkAssignedEvent
            {
                ClassRoomId = newHomework.ClassRoomId,
                SubjectId = newHomework.SubjectId,
                TeacherPersonId = teacherPersonId,
                Title = newHomework.Title,
                Description = newHomework.Description,
                HomeworkId = newHomework.HomeworkId, // مطابقة اسم الحقل المولد الكبيرة
                OccurredAt = newHomework.CreatedAt ?? DateTime.UtcNow
            });

            return true;
        }

        public async Task<TeacherWeeklyScheduleDto?> GetTeacherWeeklyScheduleAsync(int teacherPersonId, string schemeAndHost)
        {
            var teachers = await _teacherRepo.GetAllWithIncludeAndFilterAsync(
                t => t.PersonId == teacherPersonId,
                t => t.Person
            );
            var teacher = teachers.FirstOrDefault();
            if (teacher == null) return null;

            // ScheduleType = 2 => Teacher Schedule, ReferenceId = teacher.TeacherId
            var schedules = await _scheduleRepo.GetAllWithIncludeAndFilterAsync(
                s => s.ScheduleType == 2 && s.ReferenceId == teacher.TeacherId
            );
            var activeSchedule = schedules.FirstOrDefault();

            string imagePath = activeSchedule?.ImagePath ?? "uploads/schedules/default_timetable.png";
            string fullImageUrl = string.IsNullOrEmpty(imagePath)
                ? string.Empty
                : $"{schemeAndHost}/{imagePath.Replace("\\", "/").TrimStart('/')}";

            string teacherFullName = teacher.Person != null
                ? $"{teacher.Person.FirstName} {teacher.Person.LastName}".Trim().ToUpper()
                : "TEACHER";

            return new TeacherWeeklyScheduleDto
            {
                TeacherID = teacher.TeacherId,
                TeacherName = teacherFullName,
                ScheduleID = activeSchedule?.ScheduleId ?? 0,
                Title = activeSchedule?.Title ?? "Teacher Weekly Timetable",
                ScheduleType = 2,
                ImageUrl = fullImageUrl,
                UpdatedAt = activeSchedule?.UpdatedAt
            };
        }


        public async Task<IEnumerable<TeacherGradeExamScheduleDto>> GetTeacherExamSchedulesAsync(int teacherPersonId, string schemeAndHost)
        {
            var teachers = await _teacherRepo.GetAllWithIncludeAndFilterAsync(
                t => t.PersonId == teacherPersonId
            );
            var teacher = teachers.FirstOrDefault();
            if (teacher == null) return Enumerable.Empty<TeacherGradeExamScheduleDto>();

            // 1. جلب كل الشعب والمواد والصفوف التي يدرّسها هذا المعلم
            var assignments = await _classroomTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacher.TeacherId,
                ct => ct.ClassRoom,
                ct => ct.ClassRoom.Grade,
                ct => ct.Subject
            );

            if (!assignments.Any())
                return Enumerable.Empty<TeacherGradeExamScheduleDto>();

            // 2. استخراج الصفوف الفريدة التي يدرّس فيها المعلم (Distinct Grades)
            var distinctGrades = assignments
                .Where(ct => ct.ClassRoom != null && ct.ClassRoom.Grade != null)
                .Select(ct => ct.ClassRoom.Grade)
                .GroupBy(g => g.GradeId)
                .Select(group => group.First())
                .OrderBy(g => g.GradeNumber)
                .ToList();

            if (!distinctGrades.Any())
                return Enumerable.Empty<TeacherGradeExamScheduleDto>();

            var distinctGradeIds = distinctGrades.Select(g => g.GradeId).ToList();

            // 3. جلب جميع برامج الامتحانات الخاصة بهذه الصفوف المحددة
            var allExamSchedules = await _examScheduleRepo.GetAllWithIncludeAndFilterAsync(
                es => distinctGradeIds.Contains(es.GradeId),
                es => es.Grade
            );

            var resultList = new List<TeacherGradeExamScheduleDto>();

            foreach (var grade in distinctGrades)
            {
                // المواد التي يدرّسها هذا المعلم ضمن هذا الصف تحديداً
                var subjectsInGrade = assignments
                    .Where(ct => ct.ClassRoom?.GradeId == grade.GradeId && ct.Subject != null)
                    .Select(ct => ct.Subject.SubjectName)
                    .Distinct()
                    .ToList();

                // برامج الامتحانات لهذا الصف
                var gradeExams = allExamSchedules
                    .Where(es => es.GradeId == grade.GradeId)
                    .OrderByDescending(es => es.AcademicYear)
                    .ThenByDescending(es => es.Semester)
                    .Select(es =>
                    {
                        string imgPath = es.ImagePath ?? string.Empty;
                        string fullImgUrl = string.IsNullOrEmpty(imgPath)
                            ? string.Empty
                            : $"{schemeAndHost}/{imgPath.Replace("\\", "/").TrimStart('/')}";

                        return new TeacherExamScheduleItemDto
                        {
                            ExamScheduleID = es.ExamScheduleId,
                            GradeID = es.GradeId,
                            Semester = es.Semester,
                            AcademicYear = es.AcademicYear,
                            ImageUrl = fullImgUrl,
                            UpdatedAt = es.UpdatedAt
                        };
                    })
                    .ToList();

                resultList.Add(new TeacherGradeExamScheduleDto
                {
                    GradeID = grade.GradeId,
                    GradeNumber = grade.GradeNumber,
                    GradeName = $"Grade {grade.GradeNumber}",
                    SubjectsTaughtInGrade = subjectsInGrade,
                    ExamSchedules = gradeExams
                });
            }

            return resultList;
        }



    }
}
