using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class StudentExamScheduleDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = null!;
        public int GradeID { get; set; }
        public int GradeNumber { get; set; }
        public byte Semester { get; set; }
        public short AcademicYear { get; set; }
        public string ImageUrl { get; set; } = null!;




    }
}
