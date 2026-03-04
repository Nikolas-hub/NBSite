using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjangoSite
{
    public long Id { get; set; }

    public string Domain { get; set; } = null!;

    public string Name { get; set; } = null!;
}
