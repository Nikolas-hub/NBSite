using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogOrderproduct
{
    public long Id { get; set; }

    public double Price { get; set; }

    public long Quantity { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public double Volume { get; set; }

    public double Weight { get; set; }

    public virtual CatalogOrder Order { get; set; } = null!;

    public virtual CatalogProduct Product { get; set; } = null!;
}
