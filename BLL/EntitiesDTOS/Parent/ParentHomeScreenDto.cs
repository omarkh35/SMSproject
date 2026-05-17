using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class ParentDashboardDto
    {
        public string ParentName { get; set; } = null!;
        public List<DashboardAnnouncementDto> Advertisements { get; set; } = new();
        public List<LessonAssignmentStatusDto> AssignmentStatus { get; set; } = new();
    }

    public class DashboardAnnouncementDto
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
    }

    public class LessonAssignmentStatusDto
    {
        public string SubjectName { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
    }
}
