using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AuthGroup
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AuthGroupPermission> AuthGroupPermissions { get; set; } = new List<AuthGroupPermission>();

    public virtual ICollection<AuthUserGroup> AuthUserGroups { get; set; } = new List<AuthUserGroup>();
}
