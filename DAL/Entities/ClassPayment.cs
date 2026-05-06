using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class ClassPayment
{
    public int ClassPaymentId { get; set; }

    public byte Class { get; set; }

    public decimal FullAmount { get; set; }
}
