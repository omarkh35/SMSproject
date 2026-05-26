using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Teacher
{
    public class SaveAttendanceBulkDto
    {
        public int ClassRoomID { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public List<StudentAttendanceInputDto> StudentAttendances { get; set; } = new();
    }

    public class StudentAttendanceInputDto
    {
        public int StudentID { get; set; }
        public byte Status { get; set; } 
        public string? Notes { get; set; }
    }
}
