using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class ParentChatThreadDto
    {
        public int ChatRoomID { get; set; }
        public int SupervisorPersonID { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassDisplayName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ParentChatMessageDto
    {
        public long MessageID { get; set; }
        public int SenderPersonID { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsMe { get; set; }
    }

    public class ParentSendMessageDto
    {
        public int ChatRoomID { get; set; }
        public string MessageContent { get; set; } = string.Empty;
    }
}
