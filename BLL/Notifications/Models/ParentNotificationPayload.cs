using System;
using System.Collections.Generic;

namespace BLL.Notifications.Models
{
    public enum NotificationType
    {
        StudentNote = 1,
        Homework = 2,
        MarksReleased = 3,
        ChatMessage = 4
    }

    /// <summary>
    /// كائن الإشعار الموحد المرسل لتطبيق الأهل (Push Notification Payload)
    /// يحتوي على بيانات العرض المباشر وبيانات التوجيه للواجهة (Deep Linking)
    /// </summary>
    public class ParentNotificationPayload
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string TypeName => Type.ToString();
        
        /// <summary>
        /// قائمة معرّفات أولياء الأمور المستهدفين (PersonIds)
        /// </summary>
        public List<int> TargetParentPersonIds { get; set; } = new List<int>();

        /// <summary>
        /// بيانات التوجيه (Deep Linking Data) المستخدمة في الفرونت إند / الموبايل
        /// </summary>
        public NotificationRoutingData Data { get; set; } = new NotificationRoutingData();

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// تفاصيل التوجيه المرفقة مع الإشعار للفرونت إند لنقل المستخدم للشاشة المحددة فور النقر
    /// </summary>
    public class NotificationRoutingData
    {
        /// <summary>
        /// نوع الحدث للتحكم بالمسار في تطبيق الموبايل (مثال: STUDENT_NOTE, HOMEWORK, MARKS, CHAT_MESSAGE)
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// المسار الداخلي للشاشة في التطبيق (Route/Path)
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// رابط Deep Link الشامل (Custom Scheme URL) مثال: schoolapp://parent/students/5/notes/12
        /// </summary>
        public string DeepLinkUrl { get; set; } = string.Empty;

        /// <summary>
        /// رقم الطالب المرتبط بالحدث إن وجد
        /// </summary>
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }

        /// <summary>
        /// المعرف الأساسي للكيان (NoteId, HomeworkId, ChatRoomId, SubjectId)
        /// </summary>
        public long? EntityId { get; set; }

        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public int? ClassRoomId { get; set; }

        /// <summary>
        /// بيانات إضافية مخصصة حسب الحاجة
        /// </summary>
        public Dictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>();
    }
}
