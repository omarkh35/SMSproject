using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Student
{
    public class StudentProfileDto
    {

        public int StudentID { get; set; }
        public string FullName { get; set; } = null!;
        public string Grade { get; set; } = null!;       
        public string Section { get; set; } = null!;     
        public string MotherName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string ContactNumber { get; set; } = null!;
        public string? PictureUrl { get; set; }


    }
}
