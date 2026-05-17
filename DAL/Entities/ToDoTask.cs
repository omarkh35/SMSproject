using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ToDoTask
    {

        public long TaskID { get; set; }
        public int AssignedPersonID { get; set; }
        public string TaskDescription { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public int? ClassRoomID { get; set; }

        // 1 = Normal, 2 = Mid, 3 = Important
        public byte PriorityLevel { get; set; }
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }


        public virtual Person AssignedPerson { get; set; } = null!;
        public virtual ClassRoom? ClassRoom { get; set; }


    }
}
