using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AccountsProfile
{
    public long Id { get; set; }

    public bool PricesVisible { get; set; }

    public long UserId { get; set; }

    public string? Image { get; set; }

    public string? CompanyNavCode { get; set; }

    public string Fio { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Company { get; set; }

    public string? CompanyPost { get; set; }

    public string? Email { get; set; }

    public long? CityId { get; set; }

    public string? ResetPasswordKey { get; set; }

    public virtual ReferencesCity? City { get; set; }

    public virtual AuthUser User { get; set; } = null!;
}
