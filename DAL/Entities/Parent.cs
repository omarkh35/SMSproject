using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public partial class Parent
{
    [Key]
    public int Id { get; set; }


    public int PersonId { get; set; }

    public decimal WalletBalance { get; set; }


    public string FamilyCardNumber { get; set; } 

    public virtual Person Person { get; set; }


    public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();

}
