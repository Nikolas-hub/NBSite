using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjceleryTaskstate
{
    public long Id { get; set; }

    public string State { get; set; } = null!;

    public string TaskId { get; set; } = null!;

    public string? Name { get; set; }

    public DateTime Tstamp { get; set; }

    public string? Args { get; set; }

    public string? Kwargs { get; set; }

    public DateTime? Eta { get; set; }

    public DateTime? Expires { get; set; }

    public string? Result { get; set; }

    public string? Traceback { get; set; }

    public double? Runtime { get; set; }

    public long Retries { get; set; }

    public bool Hidden { get; set; }

    public long? WorkerId { get; set; }

    public virtual DjceleryWorkerstate? Worker { get; set; }
}
