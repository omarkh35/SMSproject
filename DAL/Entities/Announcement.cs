using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public int SenderPersonId { get; set; }

    public string Title { get; set; } = null!;

    public string AnnouncementBody { get; set; } = null!;

    /// <summary>
    /// 1 = Entire School, 0 = Target Specific
    /// </summary>
    public bool IsGeneral { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AnnouncementClassroom> AnnouncementClassrooms { get; set; } = new List<AnnouncementClassroom>();

    public virtual Person SenderPerson { get; set; } = null!;
}
