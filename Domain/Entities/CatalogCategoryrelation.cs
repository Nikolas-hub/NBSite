using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogCategoryrelation
{
    public long Id { get; set; }

    public long Type { get; set; }

    public long SourceId { get; set; }

    public long TargetId { get; set; }

    public virtual CatalogCategory Source { get; set; } = null!;

    public virtual CatalogCategory Target { get; set; } = null!;
}
