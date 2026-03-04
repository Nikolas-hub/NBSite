using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReferencesCity
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Active { get; set; }

    public string? KladrCode { get; set; }

    public string? DelLineCode { get; set; }

    public bool NativeDelivery { get; set; }

    public virtual ICollection<AccountsProfile> AccountsProfiles { get; set; } = new List<AccountsProfile>();

    public virtual ICollection<CatalogOrder> CatalogOrders { get; set; } = new List<CatalogOrder>();
}
