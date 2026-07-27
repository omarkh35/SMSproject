using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class StudentParent
{
    public int StudentParentId { get; set; }

    public int StudentId { get; set; }

    public string? RelationshipType { get; set; }

    public int? ParentID { get; set; }

    public virtual Parent? Parent { get; set; }

    public virtual Student Student { get; set; } = null!;
}
