using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int StudentRecordId { get; set; }

    public decimal PaymentAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    public virtual StudentRecord StudentRecord { get; set; } = null!;
}
