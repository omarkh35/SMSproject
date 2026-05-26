using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.DepartmentManager
{
    public class ClassRoomDto
    {
        public int Id { get; set; }
        public int GradeId { get; set; }
        public byte Section { get; set; } 
        public int? SupervisorId { get; set; }
        public short StartYear { get; set; }
        public int CurrentStudentsCount { get; set; }
    }

    public class ClassRoomCreateDto     
    {
        public int GradeId { get; set; }
        public byte Section { get; set; }
        public short StartYear { get; set; }
        public int? SupervisorId { get; set; }
    }

    public class ClassRoomUpdateDto 
    {
        public byte? Section { get; set; }
        public short? StartYear { get; set; }
    }

    public class StudentToClassDto
    {
        public int StudentId { get; set; }
        public int ClassRoomId { get; set; }
    }

    public class TeacherToClassDto
    {
        public int ClassroomTeacherId { get; set; }
        public int TeacherId { get; set; }
        public int ClassRoomId { get; set; }
        public int SubjectId { get; set; }
    }

    public class TeacherSupervisorDto
    {
        public int SupervisorId { get; set; }
        public int TeacherId { get; set; }
    }
    
}
