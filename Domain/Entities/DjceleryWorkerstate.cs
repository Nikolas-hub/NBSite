using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjceleryWorkerstate
{
    public long Id { get; set; }

    public string Hostname { get; set; } = null!;

    public DateTime? LastHeartbeat { get; set; }

    public virtual ICollection<DjceleryTaskstate> DjceleryTaskstates { get; set; } = new List<DjceleryTaskstate>();
}
