using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Teacher
{
    public class SaveStudentNoteDto
    {
        public int StudentID { get; set; }
        public string NoteContent { get; set; } = null!;
        public DateTime? CreatedAt { get; set; } 
    }

    public class SaveDailyLessonDto
    {
        public int ClassRoomID { get; set; }
        public int SubjectID { get; set; }
        public DateTime LessonDate { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    public class SaveHomeworkDto
    {
        public int ClassRoomID { get; set; }
        public int SubjectID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AttachmentPath { get; set; }
    }

}
