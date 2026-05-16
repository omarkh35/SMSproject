using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Accountant
{
    public int AccountantId { get; set; }

    public int PersonId { get; set; }

    public decimal? Salary { get; set; }

    public virtual Person Person { get; set; } = null!;
}
