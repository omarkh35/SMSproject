using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Teacher
{
   
        public class TeacherClassChipDto
        {
            public int ClassRoomID { get; set; }
            public int SubjectID { get; set; }
            public string DisplayText { get; set; } = null!; 
        }

        public class ClassStudentListDto
        {
            public int StudentID { get; set; }
            public string FullName { get; set; } = null!;
        }



    public class SaveGradesBulkDto
    {
        public int SubjectID { get; set; }
        public int ExamTypeID { get; set; }
        public DateOnly ExamDate { get; set; }
        public decimal FullMark { get; set; }
        public List<StudentMarkInputDto> StudentMarks { get; set; } = new();
    }

    public class StudentMarkInputDto
    {
        public int StudentID { get; set; }
        public decimal MarkValue { get; set; }
        public string? Notes { get; set; }
    }




}
