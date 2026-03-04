using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CeleryTasksetmetum
{
    public long Id { get; set; }

    public string TasksetId { get; set; } = null!;

    public string Result { get; set; } = null!;

    public DateTime DateDone { get; set; }

    public bool Hidden { get; set; }
}
