using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ContentFolder
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public long? ParentId { get; set; }

    public virtual ICollection<ContentFile> ContentFiles { get; set; } = new List<ContentFile>();

    public virtual ICollection<ContentFolder> InverseParent { get; set; } = new List<ContentFolder>();

    public virtual ContentFolder? Parent { get; set; }
}
