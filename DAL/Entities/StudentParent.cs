using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class StudentParent
{
    public int StudentParentId { get; set; }

    public int StudentId { get; set; }

    public int PersonId { get; set; }

    public string? RelationshipType { get; set; }

    public virtual Person Person { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
