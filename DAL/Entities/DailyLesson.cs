using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public partial class DailyLesson
    {

        public long DailyLessonID { get; set; }
        public int ClassRoomID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherPersonID { get; set; }
        public DateTime LessonDate { get; set; }
        public string Description { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual ClassRoom ClassRoom { get; set; } = null!;
        public virtual Subject Subject { get; set; } = null!;
        public virtual Person TeacherPerson { get; set; } = null!;

    }
}
