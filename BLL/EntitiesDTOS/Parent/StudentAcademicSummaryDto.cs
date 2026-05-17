using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class StudentAcademicSummaryDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = null!;
        public double TotalAverage { get; set; } 
        public List<string> Subjects { get; set; } = new();


    }
}
