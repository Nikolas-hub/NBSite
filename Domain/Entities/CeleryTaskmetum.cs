using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CeleryTaskmetum
{
    public long Id { get; set; }

    public string TaskId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Result { get; set; }

    public DateTime DateDone { get; set; }

    public string? Traceback { get; set; }

    public bool Hidden { get; set; }

    public string? Meta { get; set; }
}
