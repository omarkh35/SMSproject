using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Parent
{
    public class ParentChildDto
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PicturePath { get; set; }
        public string? RelationshipType { get; set; }
        public int GradeNumber { get; set; }
        public byte Section { get; set; }
        public int ClassRoomID { get; set; }
        public short StudyYear { get; set; }


    }
}
