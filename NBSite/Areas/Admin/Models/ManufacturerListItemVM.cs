namespace NBSite.Areas.Admin.Models
{
    public class ManufacturerListItemVM
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Code { get; set; }
        public string? CountryCode { get; set; }
        public string? Image { get; set; }
        public short Sort { get; set; }
        public bool Active { get; set; }
        public int ProductsCount { get; set; } // Количество товаров этого производителя
    }
}
