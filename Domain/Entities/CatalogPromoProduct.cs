using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogPromoProduct
{
    public long Id { get; set; }

    public long PromoId { get; set; }

    public long ProductId { get; set; }
}
