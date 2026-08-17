using System;
using System.Collections.Generic;

namespace BLL.Notifications.Events
{
    public interface INotificationEvent
    {
        DateTime OccurredAt { get; }
    }

    /// <summary>
    /// حدث إضافة ملاحظة للطالب من قِبل المعلم
    /// </summary>
    public class StudentNoteAddedEvent : INotificationEvent
    {
        public int StudentId { get; set; }
        public int TeacherPersonId { get; set; }
        public string NoteContent { get; set; } = string.Empty;
        public long? NoteId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// حدث إضافة واجب مدرسي جديد للشعبة الصفية
    /// </summary>
    public class HomeworkAssignedEvent : INotificationEvent
    {
        public int ClassRoomId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherPersonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HomeworkId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    public class MarksReleasedEvent : INotificationEvent
    {
        //public int ClassRoomId { get; set; }
        public int SubjectId { get; set; }
        public int ExamTypeId { get; set; }
        public DateOnly ExamDate { get; set; }
        public List<int> StudentIds { get; set; } = new List<int>();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// حدث إرسال رسالة جديدة في محادثة بين الموجه والأهل
    /// </summary>
    public class ChatMessageSentEvent : INotificationEvent
    {
        public int ChatRoomId { get; set; }
        public int SenderPersonId { get; set; }
        public int ReceiverPersonId { get; set; }
        public int StudentFocusId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public long? MessageId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
