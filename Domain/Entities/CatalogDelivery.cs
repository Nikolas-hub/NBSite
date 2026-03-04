using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogDelivery
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Handler { get; set; } = null!;

    public short Sort { get; set; }

    public virtual ICollection<CatalogOrder> CatalogOrders { get; set; } = new List<CatalogOrder>();
}
