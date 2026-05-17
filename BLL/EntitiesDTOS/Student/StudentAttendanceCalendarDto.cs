using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Student
{
    public class StudentAttendanceSummaryDto
    {
        public int AttendanceRate { get; set; }  
        public int AbsenceRate { get; set; }     
        public List<AttendanceDayDto> DaysLog { get; set; } = new();
    }

    public class AttendanceDayDto
    {
        public DateOnly Date { get; set; }
        public byte Status { get; set; }         // 1 = Present, 2 = Absent, 3 = Late, 4 = Excused
        public string? Notes { get; set; }
    }
}
