namespace NBSite.Areas.Admin.Models
{
    public class ProductListItemVM
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? CategoryName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? Image { get; set; }
    }
}
