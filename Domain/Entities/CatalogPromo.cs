using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogPromo
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CatalogCoupon> CatalogCoupons { get; set; } = new List<CatalogCoupon>();
}
