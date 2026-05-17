using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Student
{
    public class SubjectDetailedReportDto
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = null!;
        public double SubjectAverage { get; set; } 
        public List<SubjectMarkDetailsDto> MarksBreakdown { get; set; } = new();
    }

    public class SubjectMarkDetailsDto
    {
        public int MarkID { get; set; }
        public string ExamTypeName { get; set; } = null!; 
        public decimal Score { get; set; }
        public decimal OutOf { get; set; }
        public string DisplayText { get; set; } = null!;
        public DateOnly ExamDate { get; set; }
    }

}
