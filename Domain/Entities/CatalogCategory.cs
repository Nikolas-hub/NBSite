using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogCategory
{
    public long Id { get; set; }

    public bool Active { get; set; }

    public string Name { get; set; } = null!;

    public string Alias { get; set; } = null!;

    public short Sort { get; set; }

    public string? Introtext { get; set; }

    public string? Content { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaKeywords { get; set; }

    public string? MetaDescription { get; set; }

    public string? Code { get; set; }

    public string? Parent { get; set; }

    public string? SvgIcon { get; set; }

    public virtual ICollection<CatalogCategoryrelation> CatalogCategoryrelationSources { get; set; } = new List<CatalogCategoryrelation>();

    public virtual ICollection<CatalogCategoryrelation> CatalogCategoryrelationTargets { get; set; } = new List<CatalogCategoryrelation>();

    public virtual ICollection<CatalogProduct> CatalogProducts { get; set; } = new List<CatalogProduct>();
}
