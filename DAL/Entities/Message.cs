using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Message
{
    public long MessageId { get; set; }

    public int ChatRoomId { get; set; }

    public int SenderPersonId { get; set; }

    public string MessageContent { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public virtual Person SenderPerson { get; set; } = null!;
}
