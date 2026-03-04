using Domain.Entities;

namespace NBSite.Models.ViewComponents
{
    public class ShowProductVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public double Price { get; set; }
        public double OldPrice { get; set; }
        public string? Image { get; set; }
        public string? Content { get; set; }
        public string? IntroText { get; set; }
        public int Quantity { get; set; }
        public int Multiplicity { get; set; } = 1;
        public bool IsNew { get; set; }
        public bool IsPopular { get; set; }
        public bool HasReject { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }
        public string? Ean13 { get; set; }
        public string? Manual { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public long? CategoryId { get; set; }
        public CatalogCategory? Category { get; set; }
        public string? CategoryAlias { get; set; }
        public long? ManufacturerId { get; set; }
        public CatalogManufacturer? Manufacturer { get; set; }
        public List<ProductPreviewVM> RelatedProducts { get; set; } = new();
        public bool IsSubscribedToStock { get; set; }

        // Вычисляемые свойства
        public bool HasDiscount => OldPrice > 0 && OldPrice > Price;
        public int DiscountPercent => HasDiscount ? (int)Math.Round((1 - (double)Price / (double)OldPrice) * 100, 0) : 0;
        public string? Thumbnail => !string.IsNullOrEmpty(Image) ? $"{Image}?size=600x600" : null;
        public string FormattedPrice => Price.ToString("N0") + " ₽";
        public string FormattedOldPrice => OldPrice > 0 ? OldPrice.ToString("N0") + " ₽" : string.Empty;
        public bool InStock => Quantity > 0;
        public string StockStatus => Quantity > 10 ? "В наличии" : Quantity > 0 ? "Мало" : "Нет в наличии";
        public List<CatalogCategory> CategoryPath { get; set; } = new();
    }
}
