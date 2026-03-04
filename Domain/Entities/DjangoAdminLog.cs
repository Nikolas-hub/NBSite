using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DjangoAdminLog
{
    public long Id { get; set; }

    public DateTime ActionTime { get; set; }

    public string? ObjectId { get; set; }

    public string ObjectRepr { get; set; } = null!;

    public int ActionFlag { get; set; }

    public string ChangeMessage { get; set; } = null!;

    public long? ContentTypeId { get; set; }

    public long UserId { get; set; }

    public virtual DjangoContentType? ContentType { get; set; }

    public virtual AuthUser User { get; set; } = null!;
}
