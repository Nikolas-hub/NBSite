using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjkombuMessage
{
    public long Id { get; set; }

    public bool Visible { get; set; }

    public DateTime? SentAt { get; set; }

    public string Payload { get; set; } = null!;

    public long QueueId { get; set; }

    public virtual DjkombuQueue Queue { get; set; } = null!;
}
