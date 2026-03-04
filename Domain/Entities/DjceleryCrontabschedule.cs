using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjceleryCrontabschedule
{
    public long Id { get; set; }

    public string Minute { get; set; } = null!;

    public string Hour { get; set; } = null!;

    public string DayOfWeek { get; set; } = null!;

    public string DayOfMonth { get; set; } = null!;

    public string MonthOfYear { get; set; } = null!;

    public virtual ICollection<DjceleryPeriodictask1> DjceleryPeriodictask1s { get; set; } = new List<DjceleryPeriodictask1>();
}
