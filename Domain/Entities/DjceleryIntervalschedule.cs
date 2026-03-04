using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjceleryIntervalschedule
{
    public long Id { get; set; }

    public long Every { get; set; }

    public string Period { get; set; } = null!;

    public virtual ICollection<DjceleryPeriodictask1> DjceleryPeriodictask1s { get; set; } = new List<DjceleryPeriodictask1>();
}
