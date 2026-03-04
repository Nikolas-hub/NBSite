using Domain.Entities;

namespace NBSite.Models.ViewComponents
{
    public class ProductVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int? Rating { get; set; }
        public int? CategoryId { get; set; }
        public bool? OnSale { get; set; }
        public string? Description { get; set; }

        public CatalogCategory? Category { get; set; }
        public List<int>? ProductCharacteristics { get; set; }
    }
}
