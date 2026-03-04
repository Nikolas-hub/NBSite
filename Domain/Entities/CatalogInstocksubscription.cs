using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogInstocksubscription
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public long Status { get; set; }

    public long ProductId { get; set; }

    public virtual CatalogProduct Product { get; set; } = null!;
}
