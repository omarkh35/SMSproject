using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.EntitiesDTOS.Parent;


namespace BLL.EntitiesDTOS.Teacher
{
    public class TeacherDashboardDto
    {
        public string TeacherName { get; set; } = null!;
        public List<TeacherAnnouncementDto> Announcements { get; set; } = new();
    }

    public class TeacherAnnouncementDto
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string SenderName { get; set; } = null!;
        public bool IsGeneral { get; set; }
    }
}
