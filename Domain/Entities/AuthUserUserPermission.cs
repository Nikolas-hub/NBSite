using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AuthUserUserPermission
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long PermissionId { get; set; }

    public virtual AuthPermission Permission { get; set; } = null!;

    public virtual AuthUser User { get; set; } = null!;
}
