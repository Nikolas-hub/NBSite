using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AuthUser
{
    public long Id { get; set; }

    public string Password { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public bool IsSuperuser { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsStaff { get; set; }

    public bool IsActive { get; set; }

    public DateTime DateJoined { get; set; }

    public virtual AccountsProfile? AccountsProfile { get; set; }

    public virtual ICollection<AuthUserGroup> AuthUserGroups { get; set; } = new List<AuthUserGroup>();

    public virtual ICollection<AuthUserUserPermission> AuthUserUserPermissions { get; set; } = new List<AuthUserUserPermission>();

    public virtual ICollection<CatalogOrder> CatalogOrders { get; set; } = new List<CatalogOrder>();

    public virtual ICollection<DjangoAdminLog> DjangoAdminLogs { get; set; } = new List<DjangoAdminLog>();
}
