using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class AnnouncementClassroom
{
    public int AnnouncementClassroomId { get; set; }

    public int AnnouncementId { get; set; }

    public int ClassRoomId { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;

    public virtual ClassRoom ClassRoom { get; set; } = null!;
}
