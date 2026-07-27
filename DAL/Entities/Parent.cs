using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Parent
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public decimal WalletBalance { get; set; }

    public string? FamilyCardNumber { get; set; }

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
}
