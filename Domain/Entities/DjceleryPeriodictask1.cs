using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjceleryPeriodictask1
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Task { get; set; } = null!;

    public string Args { get; set; } = null!;

    public string Kwargs { get; set; } = null!;

    public string? Queue { get; set; }

    public string? Exchange { get; set; }

    public string? RoutingKey { get; set; }

    public DateTime? Expires { get; set; }

    public bool Enabled { get; set; }

    public DateTime? LastRunAt { get; set; }

    public long TotalRunCount { get; set; }

    public DateTime DateChanged { get; set; }

    public string Description { get; set; } = null!;

    public long? CrontabId { get; set; }

    public long? IntervalId { get; set; }

    public virtual DjceleryCrontabschedule? Crontab { get; set; }

    public virtual DjceleryIntervalschedule? Interval { get; set; }
}
