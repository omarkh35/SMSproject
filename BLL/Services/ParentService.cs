using BLL.EntitiesDTOS.Parent;
using BLL.EntitiesDTOS.Student;
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
    public class ParentService : IParentService
    {

        private readonly IBaseRepositories<StudentParent> _studentParentRepo;
        private readonly IBaseRepositories<Student> _studentRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classroomStudentRepo;
        private readonly IBaseRepositories<Announcement> _announcementRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<DailyLesson> _dailyLessonRepo;
        private readonly IBaseRepositories<StudentNote> _studentNoteRepo;
        private readonly IBaseRepositories<StudentAttendance> _studentAttendanceRepo;
        private readonly IBaseRepositories<Homework> _homeworkRepo;
        private readonly IBaseRepositories<ClassroomTeacher> _classroomTeacherRepo;
        private readonly IBaseRepositories<Schedule> _scheduleRepo;
        private readonly IBaseRepositories<ExamSchedule> _examScheduleRepo;
        private readonly IBaseRepositories<StudentRecord> _studentRecordRepo;
        private readonly IBaseRepositories<Mark> _markRepo;
        private readonly IBaseRepositories<GradeSubject> _gradeSubjectRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<Subject> _subjectRepo;


        public ParentService(
     IBaseRepositories<StudentParent> studentParentRepo,
     IBaseRepositories<ClassroomStudent> classroomStudentRepo,
     IBaseRepositories<Person> personRepo,
     IBaseRepositories<Announcement> announcementRepo,
     IBaseRepositories<DailyLesson> dailyLessonRepo, IBaseRepositories<StudentNote> studentNoteRepo,
        IBaseRepositories<StudentAttendance> studentAttendanceRepo
        , IBaseRepositories<Homework> homeworkRepo
        , IBaseRepositories<ClassroomTeacher> classroomTeacherRepo, IBaseRepositories<Schedule> scheduleRepo
            , IBaseRepositories<ExamSchedule> examScheduleRepo,
        IBaseRepositories<StudentRecord> studentRecordRepo, IBaseRepositories<Mark> markRepo,
        IBaseRepositories<GradeSubject> gradeSubjectRepo, IBaseRepositories<Student> studentRepo,
        IBaseRepositories<User> userRepo, IBaseRepositories<Subject> subjectRepo)
        {
            _studentParentRepo = studentParentRepo;
            _classroomStudentRepo = classroomStudentRepo;
            _personRepo = personRepo;
            _announcementRepo = announcementRepo;
            _dailyLessonRepo = dailyLessonRepo;
            _studentNoteRepo = studentNoteRepo;
            _homeworkRepo = homeworkRepo;
            _studentAttendanceRepo = studentAttendanceRepo;
            _classroomTeacherRepo = classroomTeacherRepo;
            _scheduleRepo = scheduleRepo;
            _examScheduleRepo = examScheduleRepo;
            _studentRecordRepo = studentRecordRepo;
            _markRepo = markRepo;
            _gradeSubjectRepo = gradeSubjectRepo;
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _subjectRepo = subjectRepo;
        }


        public async Task<StudentAcademicSummaryDto?> GetStudentAcademicSummaryAsync(int parentPersonId, int studentId)
        {
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
    sp => sp.Parent,
                sp => sp.Student,
                sp => sp.Student.Person
            );

            var parentLink = parentLinks.FirstOrDefault();
            if (parentLink == null)
                return null;

            var studentRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(sr => sr.StudentId == studentId);

            var activeRecord = studentRecords.OrderByDescending(sr => sr.StudyYear).FirstOrDefault();

            if (activeRecord == null)
                return null;

            var curriculumSubjects = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(
                gs => gs.GradeId == activeRecord.GradeId,
                gs => gs.Subject
            );

            var summaryDto = new StudentAcademicSummaryDto
            {
                StudentID = studentId,
                StudentName = $"{parentLink.Student.Person.FirstName} {parentLink.Student.Person.LastName}"
            };

            summaryDto.Subjects = curriculumSubjects
                .Where(gs => gs.Subject != null)
                .Select(gs => gs.Subject.SubjectName)
                .Distinct()
                .ToList();

            var approvedMarks = await _markRepo.GetAllWithIncludeAndFilterAsync(
                m => m.StudentRecordId == activeRecord.StudentRecordId
                     && m.IsApproved == true
                     && m.ExamTypeId != 1
                     && m.ExamTypeId != 2
            );

            if (approvedMarks.Any())
            {
                double cumulativeEarnedMarks = 0;
                double cumulativeFullPossibleMarks = 0;

                foreach (var mark in approvedMarks)
                {
                    if (mark.FullMark > 0)
                    {
                        cumulativeEarnedMarks += (double)mark.MarkValue;
                        cumulativeFullPossibleMarks += (double)mark.FullMark;
                    }
                }

                if (cumulativeFullPossibleMarks > 0)
                {
                    double rawPercentage = (cumulativeEarnedMarks / cumulativeFullPossibleMarks) * 100;
                    summaryDto.TotalAverage = Math.Round(rawPercentage, 2);
                }
            }
            else
            {
                summaryDto.TotalAverage = 0.0;
            }

            return summaryDto;
        }

        public async Task<StudentExamScheduleDto?> GetStudentExamScheduleAsync(int parentPersonId, int studentId, string schemeAndHost)
        {
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
    sp => sp.Parent,
                sp => sp.Student,
                sp => sp.Student.Person
            );
            var parentLink = parentLinks.FirstOrDefault();
            if (parentLink == null)
                return null; 

            var studentRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(
                sr => sr.StudentId == studentId,
                sr => sr.Grade
            );
            var activeRecord = studentRecords.OrderByDescending(sr => sr.StudyYear).FirstOrDefault();
            if (activeRecord == null)
                return null; 

            var examSchedules = await _examScheduleRepo.GetAllWithIncludeAndFilterAsync(
                es => es.GradeId == activeRecord.GradeId && es.AcademicYear == activeRecord.StudyYear
            );

            
            var activeExamSchedule = examSchedules.OrderByDescending(es => es.Semester).FirstOrDefault();


            string imagePath = activeExamSchedule?.ImagePath ?? "uploads/exams/default_exam_schedule.png";
            string fullImageUrl = $"{schemeAndHost}/{imagePath.Replace("\\", "/")}";

            return new StudentExamScheduleDto
            {
                StudentID = studentId,
                StudentName = $"{parentLink.Student.Person.FirstName} {parentLink.Student.Person.LastName}",
                GradeID = activeRecord.GradeId,
                GradeNumber = activeRecord.Grade?.GradeNumber ?? 0,
                Semester = activeExamSchedule?.Semester ?? 1, 
                AcademicYear = activeRecord.StudyYear,
                ImageUrl = fullImageUrl
            };
        }

        public async Task<StudentScheduleDto?> GetStudentWeeklyScheduleAsync(int parentPersonId, int studentId, string schemeAndHost)
        {
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
    sp => sp.Parent,
                sp => sp.Student,
                sp => sp.Student.Person
            );

            var parentLink = parentLinks.FirstOrDefault();

            if (parentLink == null)
                return null; 

            var classroomLinks = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == studentId,
                cs => cs.ClassRoom,
                cs => cs.ClassRoom.Grade
            );

            var currentClassroom = classroomLinks.Select(cs => cs.ClassRoom).FirstOrDefault();
            if (currentClassroom == null)
                return null;

            // ScheduleType = 1 => Classroom
            var schedules = await _scheduleRepo.GetAllWithIncludeAndFilterAsync(
                s => s.ScheduleType == 1 && s.ReferenceId == currentClassroom.ClassRoomId
            );
            var activeSchedule = schedules.FirstOrDefault();

            
            string imagePath = activeSchedule?.ImagePath ?? "uploads/schedules/default_timetable.png";

            string fullImageUrl = $"{schemeAndHost}/{imagePath.Replace("\\", "/")}";

            return new StudentScheduleDto
            {
                StudentID = studentId,
                StudentName = $"{parentLink.Student.Person.FirstName} {parentLink.Student.Person.LastName}",
                ClassRoomID = currentClassroom.ClassRoomId,
                GradeAndSection = $"Grade {currentClassroom.Grade?.GradeNumber ?? 0} - Section {currentClassroom.Section}",
                ScheduleTitle = activeSchedule?.Title ?? "Weekly Class Schedule",
                ImageUrl = fullImageUrl
            };
        }


        public async Task<IEnumerable<ParentChildDto>> GetMyChildrenAsync(int parentPersonId)
        {

            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId,
    sp => sp.Parent,
                sp => sp.Student,
                sp => sp.Student.Person,
                sp => sp.Student.StudentRecords
            );

            var childrenDtos = new List<ParentChildDto>();

            short currentYear = (short)DateTime.UtcNow.Year;

            foreach (var link in parentLinks)
            {
                var student = link.Student;


                var activeRecord = student.StudentRecords
                    .OrderByDescending(r => r.StudyYear)
                    .FirstOrDefault();


                var classroomLinks = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                    cs => cs.StudentId == student.StudentId,
                    cs => cs.ClassRoom,
                    cs => cs.ClassRoom.Grade
                );

                var activeClassroom = classroomLinks
                    .Select(cs => cs.ClassRoom)
                    .FirstOrDefault(c => activeRecord != null && c.GradeId == activeRecord.GradeId);

                childrenDtos.Add(new ParentChildDto
                {
                    StudentID = student.StudentId,
                    FirstName = student.Person.FirstName,
                    LastName = student.Person.LastName,
                    MotherName = student.MotherName,
                    Address = student.Address,
                    PicturePath = student.Picture,
                    RelationshipType = link.RelationshipType ?? "Parent",


                    GradeNumber = activeClassroom?.Grade?.GradeNumber ?? 0,
                    Section = activeClassroom?.Section ?? 0,
                    ClassRoomID = activeClassroom?.ClassRoomId ?? 0,
                    StudyYear = activeRecord?.StudyYear ?? currentYear
                });
            }

            return childrenDtos;
        }



        public async Task<ParentDashboardDto> GetParentDashboardAsync(int parentPersonId)
        {
            var dashboard = new ParentDashboardDto();

            var person = (await _personRepo.GetAllWithIncludeAndFilterAsync(p => p.PersonId == parentPersonId)).FirstOrDefault();
            if (person != null)
            {
                dashboard.ParentName = $"{person.FirstName} {person.LastName}".ToUpper();
            }

            var announcements = await _announcementRepo.GetAllWithIncludeAndFilterAsync(a => a.IsGeneral == true);
            dashboard.Advertisements = announcements.Select(a => new DashboardAnnouncementDto
            {
                AnnouncementID = a.AnnouncementId,
                Title = a.Title,
                Body = a.AnnouncementBody
            }).ToList();

            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
    sp => sp.Parent.PersonId == parentPersonId,
    sp => sp.Parent
);
            var childrenIds = parentLinks.Select(sp => sp.StudentId).ToList();

            var classroomLinks = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(cs => childrenIds.Contains(cs.StudentId));
            var activeClassroomIds = classroomLinks.Select(cs => cs.ClassRoomId).Distinct().ToList();

            var today = DateTime.UtcNow.Date;
            var lessonsToday = await _dailyLessonRepo.GetAllWithIncludeAndFilterAsync(
                l => activeClassroomIds.Contains(l.ClassRoomID) && l.LessonDate == today,
                l => l.Subject
            );

            dashboard.AssignmentStatus = lessonsToday.Select(l => new LessonAssignmentStatusDto
            {
                SubjectName = l.Subject?.SubjectName ?? "Subject",
                ShortDescription = l.Description.Length > 60 ? l.Description.Substring(0, 57) + "..." : l.Description
            }).ToList();

            return dashboard;
        }

        public async Task<StudentBagDetailsDto?> GetStudentBagDetailsAsync(int parentPersonId, int studentId)
        {
            //منرجع مناكد انو الابن للاب
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
    sp => sp.Parent
            );
            if (!parentLinks.Any()) return null;

            var detailsDto = new StudentBagDetailsDto();

            
            var notes = await _studentNoteRepo.GetAllWithIncludeAndFilterAsync(
                n => n.StudentId == studentId,
                n => n.TeacherPerson
            );

            foreach (var note in notes)
            {
                var teacherClassroomInfo = await _classroomTeacherRepo.GetAllWithIncludeAndFilterAsync(
                    ct => ct.Teacher.PersonId == note.TeacherPersonId,
                    ct => ct.Subject
                );

                var subjectName = teacherClassroomInfo.Select(ct => ct.Subject.SubjectName).FirstOrDefault() ?? "Teacher";

                detailsDto.Notes.Add(new StudentNoteDto
                {
                    TeacherName = note.TeacherPerson != null ? $"MS. {note.TeacherPerson.FirstName} {note.TeacherPerson.LastName}".ToUpper() : "UNKNOWN TEACHER",
                    SubjectTitle = $"{subjectName} TEACHER".ToUpper(),
                    NoteContent = note.NoteContent
                });
            }

            
            var attendanceLogs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(a => a.StudentId == studentId);
            if (attendanceLogs.Any())
            {
                double totalDays = attendanceLogs.Count();
                double presentDays = attendanceLogs.Count(a => a.Status == 1 || a.Status == 3 || a.Status == 4);
                detailsDto.AttendancePercentage = (int)Math.Round((presentDays / totalDays) * 100);
            }
            else
            {
                detailsDto.AttendancePercentage = 100; 
            }

            
            var classroomStudentLinks = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(cs => cs.StudentId == studentId);
            var activeClassroomId = classroomStudentLinks.Select(cs => cs.ClassRoomId).FirstOrDefault();

            if (activeClassroomId != 0)
            {
                var homeworkList = await _homeworkRepo.GetAllWithIncludeAndFilterAsync(
                    h => h.ClassRoomId == activeClassroomId,
                    h => h.Subject
                );

                detailsDto.Homeworks = homeworkList.Select(h => new StudentHomeworkDto
                {
                    SubjectName = h.Subject?.SubjectName ?? "General Assignment",
                    Details = h.Description ?? h.Title
                }).ToList();
            }

            return detailsDto;
        }


        public async Task<StudentProfileDto?> GetStudentProfileAsync(int parentPersonId, int studentId, string schemeAndHost)
        {
            var allAssociatedParents = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
     sp => sp.StudentId == studentId,
     sp => sp.Parent,
     sp => sp.Parent.Person
 );


            if (!allAssociatedParents.Any(sp => sp.Parent.PersonId == parentPersonId))
            {
                return null;
            }

            var students = await _studentRepo.GetAllWithIncludeAndFilterAsync(
                s => s.StudentId == studentId,
                s => s.Person
            );
            var student = students.FirstOrDefault();
            if (student == null) return null;

            var classroomLinks = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == studentId,
                cs => cs.ClassRoom,
                cs => cs.ClassRoom.Grade
            );
            var activeClassroom = classroomLinks.Select(cs => cs.ClassRoom).FirstOrDefault();

            var primaryContactRelation = allAssociatedParents.FirstOrDefault();
            string resolvedContactNumber = "N/A"; 

            if (primaryContactRelation != null)
            {
                var relatedUsers = await _userRepo.GetAllWithIncludeAndFilterAsync(
                    u => u.PersonId == primaryContactRelation.Parent.PersonId
                );
                var activeUser = relatedUsers.FirstOrDefault();
                if (activeUser != null)
                {
                    resolvedContactNumber = activeUser.PhoneNumber;
                }
            }

            string imagePath = student.Picture ?? "uploads/profiles/default_student.png";
            string fullImageUrl = $"{schemeAndHost}/{imagePath.Replace("\\", "/")}";

            return new StudentProfileDto
            {
                StudentID = student.StudentId,
                FullName = $"{student.Person.FirstName} {student.Person.SecondName} {student.Person.LastName}".ToUpper(),
                Grade = GetGradeNameWord(activeClassroom?.Grade?.GradeNumber),
                Section = GetSectionNameWord(activeClassroom?.Section),
                MotherName = student.MotherName.ToUpper(),
                FatherName = student.Person.SecondName.ToUpper(), 
                ContactNumber = resolvedContactNumber,
                PictureUrl = fullImageUrl
            };
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


        public async Task<StudentAttendanceSummaryDto?> GetStudentAttendanceCalendarAsync(int parentPersonId, int studentId, int year, int month)
        {
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
    // FIX: Route the PersonID filter through the Parent relation column
    sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
    sp => sp.Parent
);

            if (!parentLinks.Any()) 
                return null; 

            var logs = await _studentAttendanceRepo.GetAllWithIncludeAndFilterAsync(
                a => a.StudentId == studentId && a.AttendanceDate.Year == year && a.AttendanceDate.Month == month
            );

            var summaryDto = new StudentAttendanceSummaryDto();

            summaryDto.DaysLog = logs.Select(a => new AttendanceDayDto
            {
                Date = a.AttendanceDate,
                Status = a.Status, // 1, 2, 3, or 4
                Notes = a.Notes
            }).OrderBy(d => d.Date).ToList();

            if (logs.Any())
            {
                double totalDays = logs.Count();

                double attendedDays = logs.Count(a => a.Status == 1 || a.Status == 3 || a.Status == 4);
                double absentDays = logs.Count(a => a.Status == 2);

                summaryDto.AttendanceRate = (int)Math.Round((attendedDays / totalDays) * 100);
                summaryDto.AbsenceRate = (int)Math.Round((absentDays / totalDays) * 100);
            }
            else
            {
                summaryDto.AttendanceRate = 100;
                summaryDto.AbsenceRate = 0;
            }

            return summaryDto;
        }



        public async Task<SubjectDetailedReportDto?> GetSubjectDetailedReportAsync(int parentPersonId, int studentId, int subjectId)
        {
            var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                sp => sp.Parent.PersonId == parentPersonId && sp.StudentId == studentId,
                sp => sp.Parent,
                sp => sp.Student,
                sp => sp.Student.Person
            );
            if (!parentLinks.Any()) return null;

            var studentRecords = await _studentRecordRepo.GetAllWithIncludeAndFilterAsync(sr => sr.StudentId == studentId);
            var activeRecord = studentRecords.OrderByDescending(sr => sr.StudyYear).FirstOrDefault();
            if (activeRecord == null) return null;

            var subjects = await _subjectRepo.GetAllWithIncludeAndFilterAsync(s => s.SubjectId == subjectId);
            var subject = subjects.FirstOrDefault();
            if (subject == null) return null;

            var approvedSubjectMarks = await _markRepo.GetAllWithIncludeAndFilterAsync(
                m => m.StudentRecordId == activeRecord.StudentRecordId
                     && m.SubjectId == subjectId
                     && m.IsApproved == true,
                m => m.ExamType
            );

            var reportDto = new SubjectDetailedReportDto
            {
                StudentID = studentId,
                SubjectID = subjectId,
                SubjectName = subject.SubjectName.ToUpper()
            };

            double cumulativeEarned = 0;
            double cumulativePossible = 0;

            //  0= Male, 1 =Female
            string pronoun = parentLinks.First().Student.Person.Gender ? "SHE" : "HE";

            foreach (var m in approvedSubjectMarks)
            {
                if (m.FullMark > 0)
                {
                    cumulativeEarned += (double)m.MarkValue;
                    cumulativePossible += (double)m.FullMark;
                }

                reportDto.MarksBreakdown.Add(new SubjectMarkDetailsDto
                {
                    MarkID = m.MarkId,
                    ExamTypeName = m.ExamType?.ExamTypeName?.ToUpper() ?? "EXAM",
                    Score = m.MarkValue,
                    OutOf = m.FullMark,
                    DisplayText = $"{pronoun} SCORED {Math.Round(m.MarkValue, 0)} OUT OF {Math.Round(m.FullMark, 0)}.",

                    ExamDate = m.ExamDate
                });
            }

            if (cumulativePossible > 0)
            {
                reportDto.SubjectAverage = Math.Round((cumulativeEarned / cumulativePossible) * 100, 1);
            }
            else
            {
                reportDto.SubjectAverage = 0.0;
            }

            return reportDto;
        }


    }
}
