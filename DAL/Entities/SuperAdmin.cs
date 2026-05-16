using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class SuperAdmin
{
    public int AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string StaticPassword { get; set; } = null!;
}
