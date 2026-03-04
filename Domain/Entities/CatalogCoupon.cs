using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogCoupon
{
    public long Id { get; set; }

    public bool IsActive { get; set; }

    public string Code { get; set; } = null!;

    public long PromoId { get; set; }

    public virtual CatalogPromo Promo { get; set; } = null!;
}
