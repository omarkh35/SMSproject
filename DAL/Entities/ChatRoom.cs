using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ChatRoom
{
    public int ChatRoomId { get; set; }

    public int StudentFocusId { get; set; }

    public int SupervisorPersonId { get; set; }

    public int ParentPersonId { get; set; }

    public string? LastMessageContent { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Person ParentPerson { get; set; } = null!;

    public virtual Student StudentFocus { get; set; } = null!;

    public virtual Person SupervisorPerson { get; set; } = null!;
}
