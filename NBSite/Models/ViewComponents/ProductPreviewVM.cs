namespace NBSite.Models.ViewComponents
{
    public class ProductPreviewVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public double Price { get; set; }
        public double OldPrice { get; set; }
        public string? Image { get; set; }
        public string? IntroText { get; set; }
        public bool IsNew { get; set; }
        public bool IsPopular { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryAlias { get; set; }
        public string? ManufacturerName { get; set; }

        // Добавляем новые свойства
        public long? CategoryId { get; set; }
        public long? ManufacturerId { get; set; }
        public int Quantity { get; set; }
        public int Multiplicity { get; set; } = 1;

        // Вычисляемые свойства (только get)
        //public bool HasDiscount => OldPrice > 0 && OldPrice > Price;
        //public int DiscountPercent => HasDiscount ? (int)Math.Round((1 - (double)Price / (double)OldPrice) * 100, 0) : 0;
        public string? Thumbnail => !string.IsNullOrEmpty(Image) ? $"{Image}?size=300x300" : null;
        public string FormattedPrice => Price.ToString("N2") + " ₽";
        public string FormattedOldPrice => OldPrice > 0 ? OldPrice.ToString("N2") + " ₽" : string.Empty;

        // Методы для проверки наличия
        public bool InStock => Quantity > 0;
        public string StockStatus => Quantity > 10 ? "В наличии" : Quantity > 0 ? "Мало" : "Нет в наличии";
        public bool HasDiscount { get; set; }
        public decimal? DiscountedPrice { get; set; } // итоговая цена со скидкой
        public int? DiscountPercent { get; set; } // процент скидки (для бейджа)
        public string DiscountBadgeText => HasDiscount
            ? (DiscountPercent.HasValue ? $"-{DiscountPercent}%" : "Скидка")
            : "";
    }
}
