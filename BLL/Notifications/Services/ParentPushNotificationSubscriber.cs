using BLL.Notifications.Events;
using BLL.Notifications.Interfaces;
using BLL.Notifications.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Notifications.Services
{
    /// <summary>
    /// المشترك الأساسي لمعالجة أحداث تطبيق الأهل (Parent Push Notification Subscriber)
    /// يتفاعل فور وقوع أي حدث ويحدد أولياء الأمور المعنيين ويبني حزم الإشعارات الفورية
    /// مع بيانات التوجيه (Deep Linking) لإرسالها عبر Dispatcher دون تخزين في قاعدة البيانات.
    /// </summary>
    public class ParentPushNotificationSubscriber :
        INotificationSubscriber<StudentNoteAddedEvent>,
        INotificationSubscriber<HomeworkAssignedEvent>,
        INotificationSubscriber<MarksReleasedEvent>,
        INotificationSubscriber<ChatMessageSentEvent>
    {
        private readonly IBaseRepositories<StudentParent> _studentParentRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classroomStudentRepo;
        private readonly IBaseRepositories<Student> _studentRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<Subject> _subjectRepo;
        private readonly IBaseRepositories<ExamType> _examTypeRepo;
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IParentPushNotificationDispatcher _dispatcher;
        private readonly ILogger<ParentPushNotificationSubscriber> _logger;

        public ParentPushNotificationSubscriber(
            IBaseRepositories<StudentParent> studentParentRepo,
            IBaseRepositories<ClassroomStudent> classroomStudentRepo,
            IBaseRepositories<Student> studentRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<Subject> subjectRepo,
            IBaseRepositories<ExamType> examTypeRepo,
            IBaseRepositories<ClassRoom> classRoomRepo,
            IParentPushNotificationDispatcher dispatcher,
            ILogger<ParentPushNotificationSubscriber> logger)
        {
            _studentParentRepo = studentParentRepo;
            _classroomStudentRepo = classroomStudentRepo;
            _studentRepo = studentRepo;
            _personRepo = personRepo;
            _subjectRepo = subjectRepo;
            _examTypeRepo = examTypeRepo;
            _classRoomRepo = classRoomRepo;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        // =========================================================================
        // الحالة 1: عندما يرسل الأستاذ ملاحظة (Note) عن الطالب
        // =========================================================================
        public async Task HandleAsync(StudentNoteAddedEvent domainEvent)
        {
            try
            {
                // 1. جلب أولياء أمور الطالب
                var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                    sp => sp.StudentId == domainEvent.StudentId,
                    sp => sp.Parent
                );

                var parentPersonIds = parentLinks
                    .Where(sp => sp.Parent != null)
                    .Select(sp => sp.Parent!.PersonId)
                    .Distinct()
                    .ToList();

                if (!parentPersonIds.Any()) return;

                // 2. جلب اسم الطالب واسم المعلم
                var students = await _studentRepo.GetAllWithIncludeAndFilterAsync(
                    s => s.StudentId == domainEvent.StudentId,
                    s => s.Person
                );
                var student = students.FirstOrDefault();
                string studentName = student?.Person != null
                    ? $"{student.Person.FirstName} {student.Person.LastName}".Trim()
                    : "ابنكم";

                var teacher = await _personRepo.GetByIdAsync(domainEvent.TeacherPersonId);
                string teacherName = teacher != null
                    ? $"{teacher.FirstName} {teacher.LastName}".Trim()
                    : "المعلم";

                string notePreview = domainEvent.NoteContent.Length > 80
                    ? domainEvent.NoteContent.Substring(0, 77) + "..."
                    : domainEvent.NoteContent;

                // 3. بناء حمولة الإشعار الفوري مع روابط التوجيه
                var payload = new ParentNotificationPayload
                {
                    Title = $"ملاحظة جديدة حول الطالب: {studentName}",
                    Body = $"أضاف الأستاذ {teacherName} ملاحظة جديدة: \"{notePreview}\"",
                    Type = NotificationType.StudentNote,
                    TargetParentPersonIds = parentPersonIds,
                    Data = new NotificationRoutingData
                    {
                        ActionType = "STUDENT_NOTE",
                        Route = $"/parent/children/{domainEvent.StudentId}/bag",
                        DeepLinkUrl = $"schoolapp://parent/students/{domainEvent.StudentId}/bag?tab=notes&noteId={domainEvent.NoteId}",
                        StudentId = domainEvent.StudentId,
                        StudentName = studentName,
                        EntityId = domainEvent.NoteId,
                        AdditionalParameters = new Dictionary<string, string>
                        {
                            { "tab", "notes" },
                            { "teacherName", teacherName }
                        }
                    }
                };

                await _dispatcher.DispatchAsync(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PushNotification] Error processing StudentNoteAddedEvent for Student {StudentId}", domainEvent.StudentId);
            }
        }

        // =========================================================================
        // الحالة 2: عندما يرسل الأستاذ واجباً مدرسياً (Homework) للشعبة
        // =========================================================================
        public async Task HandleAsync(HomeworkAssignedEvent domainEvent)
        {
            try
            {
                // 1. جلب الطلاب المسجلين في الشعبة الصفية المستهدفة
                var classroomStudents = await _classroomStudentRepo.GetAllWithIncludeAndFilterAsync(
                    cs => cs.ClassRoomId == domainEvent.ClassRoomId
                );
                var studentIds = classroomStudents.Select(cs => cs.StudentId).Distinct().ToList();
                if (!studentIds.Any()) return;

                // 2. جلب جميع أولياء أمور هؤلاء الطلاب
                var parentLinks = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                    sp => studentIds.Contains(sp.StudentId),
                    sp => sp.Parent
                );

                var parentPersonIds = parentLinks
                    .Where(sp => sp.Parent != null)
                    .Select(sp => sp.Parent!.PersonId)
                    .Distinct()
                    .ToList();

                if (!parentPersonIds.Any()) return;

                // 3. جلب تفاصيل المادة والمعلم والشعبة
                var subject = await _subjectRepo.GetByIdAsync(domainEvent.SubjectId);
                string subjectName = subject?.SubjectName ?? "المادة";

                var teacher = await _personRepo.GetByIdAsync(domainEvent.TeacherPersonId);
                string teacherName = teacher != null
                    ? $"{teacher.FirstName} {teacher.LastName}".Trim()
                    : "المعلم";

                var classroom = await _classRoomRepo.GetByIdAsync(domainEvent.ClassRoomId);
                string classDisplay = classroom != null ? $"شعبة {classroom.Section}" : "";

                // 4. بناء حمولة الإشعار
                var payload = new ParentNotificationPayload
                {
                    Title = $"واجب مدرسي جديد - {subjectName}",
                    Body = $"أضاف الأستاذ {teacherName} واجباً جديداً: {domainEvent.Title}",
                    Type = NotificationType.Homework,
                    TargetParentPersonIds = parentPersonIds,
                    Data = new NotificationRoutingData
                    {
                        ActionType = "HOMEWORK",
                        Route = $"/parent/children/bag",
                        DeepLinkUrl = $"schoolapp://parent/homework/{domainEvent.HomeworkId}?classRoomId={domainEvent.ClassRoomId}",
                        ClassRoomId = domainEvent.ClassRoomId,
                        SubjectId = domainEvent.SubjectId,
                        SubjectName = subjectName,
                        EntityId = domainEvent.HomeworkId,
                        AdditionalParameters = new Dictionary<string, string>
                        {
                            { "tab", "homework" },
                            { "homeworkTitle", domainEvent.Title },
                            { "teacherName", teacherName }
                        }
                    }
                };

                await _dispatcher.DispatchAsync(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PushNotification] Error processing HomeworkAssignedEvent for ClassRoom {ClassRoomId}", domainEvent.ClassRoomId);
            }
        }

        // =========================================================================
        // الحالة 3: عندما يتم إرسال أو نشر العلامات للطلاب
        // =========================================================================
        public async Task HandleAsync(MarksReleasedEvent domainEvent)
        {
            try
            {
                if (!domainEvent.StudentIds.Any()) return;

                // 1. جلب اسم المادة ونوع الامتحان
                var subject = await _subjectRepo.GetByIdAsync(domainEvent.SubjectId);
                string subjectName = subject?.SubjectName ?? "المادة الدراسية";

                var examType = await _examTypeRepo.GetByIdAsync(domainEvent.ExamTypeId);
                string examTypeName = examType?.ExamTypeName ?? "الامتحان";

                // 2. معالجة إشعار مخصص لكل طالب وولي أمره لضمان التوجيه الصحيح لصفحة الطالب
                var studentParents = await _studentParentRepo.GetAllWithIncludeAndFilterAsync(
                    sp => domainEvent.StudentIds.Contains(sp.StudentId),
                    sp => sp.Parent,
                    sp => sp.Student,
                    sp => sp.Student.Person
                );

                var groupedByStudent = studentParents.GroupBy(sp => sp.StudentId);

                foreach (var studentGroup in groupedByStudent)
                {
                    int studentId = studentGroup.Key;
                    var firstLink = studentGroup.FirstOrDefault();
                    string studentName = firstLink?.Student?.Person != null
                        ? $"{firstLink.Student.Person.FirstName} {firstLink.Student.Person.LastName}".Trim()
                        : "الطالب";

                    var parentIds = studentGroup
                        .Where(sp => sp.Parent != null)
                        .Select(sp => sp.Parent!.PersonId)
                        .Distinct()
                        .ToList();

                    if (!parentIds.Any()) continue;

                    var payload = new ParentNotificationPayload
                    {
                        Title = $"صدور درجات: {subjectName}",
                        Body = $"تم رصد واعتماد درجات ({examTypeName}) للطالب {studentName}.",
                        Type = NotificationType.MarksReleased,
                        TargetParentPersonIds = parentIds,
                        Data = new NotificationRoutingData
                        {
                            ActionType = "MARKS_RELEASED",
                            Route = $"/parent/student/{studentId}/subject/{domainEvent.SubjectId}/detailes",
                            DeepLinkUrl = $"schoolapp://parent/students/{studentId}/academic-summary?subjectId={domainEvent.SubjectId}",
                            StudentId = studentId,
                            StudentName = studentName,
                            SubjectId = domainEvent.SubjectId,
                            SubjectName = subjectName,
                            //ClassRoomId = domainEvent.ClassRoomId,
                            AdditionalParameters = new Dictionary<string, string>
                            {
                                { "examTypeName", examTypeName },
                                { "examDate", domainEvent.ExamDate.ToString("yyyy-MM-dd") }
                            }
                        }
                    };

                    await _dispatcher.DispatchAsync(payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PushNotification] Error processing MarksReleasedEvent for Subject {SubjectId}", domainEvent.SubjectId);
            }
        }

        // =========================================================================
        // الحالة 4: عندما تصل رسالة جديدة في المحادثة
        // =========================================================================
        public async Task HandleAsync(ChatMessageSentEvent domainEvent)
        {
            try
            {
                // إذا كان المرسل هو نفسه المستلم (فحص أمان)
                if (domainEvent.SenderPersonId == domainEvent.ReceiverPersonId) return;

                // 1. جلب اسم المرسل (سواء كان موجهاً أو إدارة)
                var sender = await _personRepo.GetByIdAsync(domainEvent.SenderPersonId);
                string senderName = sender != null
                    ? $"{sender.FirstName} {sender.LastName}".Trim()
                    : "المشرف المدرسي";

                // 2. جلب اسم الطالب المرتبط بالمحادثة إن وجد
                string studentName = string.Empty;
                if (domainEvent.StudentFocusId > 0)
                {
                    var students = await _studentRepo.GetAllWithIncludeAndFilterAsync(
                        s => s.StudentId == domainEvent.StudentFocusId,
                        s => s.Person
                    );
                    var student = students.FirstOrDefault();
                    if (student?.Person != null)
                    {
                        studentName = $"{student.Person.FirstName} {student.Person.LastName}".Trim();
                    }
                }

                string preview = domainEvent.MessageContent.Length > 80
                    ? domainEvent.MessageContent.Substring(0, 77) + "..."
                    : domainEvent.MessageContent;

                string bodyText = !string.IsNullOrEmpty(studentName)
                    ? $"بخصوص الطالب ({studentName}): {preview}"
                    : preview;

                // 3. بناء حمولة الإشعار
                var payload = new ParentNotificationPayload
                {
                    Title = $"رسالة جديدة من {senderName}",
                    Body = bodyText,
                    Type = NotificationType.ChatMessage,
                    TargetParentPersonIds = new List<int> { domainEvent.ReceiverPersonId },
                    Data = new NotificationRoutingData
                    {
                        ActionType = "CHAT_MESSAGE",
                        Route = $"/parent/chat-history/{domainEvent.ChatRoomId}",
                        DeepLinkUrl = $"schoolapp://parent/chats/{domainEvent.ChatRoomId}",
                        EntityId = domainEvent.ChatRoomId,
                        StudentId = domainEvent.StudentFocusId > 0 ? domainEvent.StudentFocusId : null,
                        StudentName = !string.IsNullOrEmpty(studentName) ? studentName : null,
                        AdditionalParameters = new Dictionary<string, string>
                        {
                            { "chatRoomId", domainEvent.ChatRoomId.ToString() },
                            { "senderName", senderName }
                        }
                    }
                };

                await _dispatcher.DispatchAsync(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PushNotification] Error processing ChatMessageSentEvent for ChatRoom {ChatRoomId}", domainEvent.ChatRoomId);
            }
        }
    }
}
