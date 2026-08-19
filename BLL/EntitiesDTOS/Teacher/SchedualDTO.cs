using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Teacher
{
    public class TeacherWeeklyScheduleDto
    {
        public int TeacherID { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int ScheduleID { get; set; }
        public string Title { get; set; } = string.Empty;
        public byte ScheduleType { get; set; } = 2; // 2 = Teacher
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }

    public class TeacherGradeExamScheduleDto
    {
        public int GradeID { get; set; }
        public int GradeNumber { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public List<string> SubjectsTaughtInGrade { get; set; } = new();
        public List<TeacherExamScheduleItemDto> ExamSchedules { get; set; } = new();
    }

    public class TeacherExamScheduleItemDto
    {
        public int ExamScheduleID { get; set; }
        public int GradeID { get; set; }
        public byte Semester { get; set; }
        public short AcademicYear { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }
}
