using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.SchoolAdmin
{

    
    public class SubjectDto 
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = null!;
    }

    public class SubjectCreateDto 
    {
        public string SubjectName { get; set; } = null!;
    }

    public class SubjectUpdateDto
    {
        public string SubjectName { get; set; } = null!;
    }

    public class StaffDto
    {
        public int Id { get; set; } 
        public int PersonId { get; set; }
        public string FullName { get; set; } = null!;
        public decimal? Salary { get; set; }
        public string Role { get; set; } = null!;
        public string? DepartmentManagerName { get; set; } 
    }

    public class DepartmentManagerCreateDto
    {
        public int PersonId { get; set; }
        public decimal Salary { get; set; }
    }

    public class SupervisorCreateDto
    {
        public int PersonId { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentManagerId { get; set; }
    }

    public class StaffUpdateDto
    {
        public decimal? Salary { get; set; }
    }

    public class GradeSubjectDto
    {
        public int GradeId { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public string? GradeName { get; set; }
    }

}