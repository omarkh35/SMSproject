using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class StudentScheduleDto
    {

        public int StudentID { get; set; }
        public string StudentName { get; set; } = null!;
        public int ClassRoomID { get; set; }
        public string GradeAndSection { get; set; } = null!;
        public string ScheduleTitle { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;



    }
}
