using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Teacher
{
    public class TeacherDetailedProfileDto
    {

       
            public int TeacherID { get; set; }
            public string FullName { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
            public string? Email { get; set; }


            public List<string> Schedules { get; set; } = new();
            public List<string> SubjectsTaught { get; set; } = new();
        


    }
}
