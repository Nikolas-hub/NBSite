using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class SearchSearchlogentry
{
    public long Id { get; set; }

    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
