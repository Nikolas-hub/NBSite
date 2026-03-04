using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ContentFile
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Attachment { get; set; }

    public long FolderId { get; set; }

    public virtual ContentFolder Folder { get; set; } = null!;
}
