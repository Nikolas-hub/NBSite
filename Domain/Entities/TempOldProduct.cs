using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TempOldProduct
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

    public double Price { get; set; }

    public long Quantity { get; set; }

    public string? Ean13 { get; set; }

    public double Volume { get; set; }

    public double Weight { get; set; }

    public long? CategoryId { get; set; }

    public long? ManufacturerId { get; set; }

    public string? Manual { get; set; }

    public long Multiplicity { get; set; }

    public bool New { get; set; }

    public bool Popular { get; set; }

    public string? Image { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public bool HasReject { get; set; }

    public double OldPrice { get; set; }

    public string? SearchKeywords { get; set; }
}
