using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    

        public class StudentBagDetailsDto
        {
            public List<StudentNoteDto> Notes { get; set; } = new();
            public int AttendancePercentage { get; set; }
            public List<StudentHomeworkDto> Homeworks { get; set; } = new();
        }

        public class StudentNoteDto
        {
            public string TeacherName { get; set; } = null!;
            //منبعت اسم المادة مو رقمها 

            public string SubjectTitle { get; set; } = null!; 
            public string NoteContent { get; set; } = null!;
        }

        public class StudentHomeworkDto
        {
            public string SubjectName { get; set; } = null!;
            public string Details { get; set; } = null!;
        }


    
}
