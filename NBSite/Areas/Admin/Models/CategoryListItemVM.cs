namespace NBSite.Areas.Admin.Models
{
    public class CategoryListItemVM
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Code { get; set; }
        public string? ParentName { get; set; } // Название родительской категории
        public int Sort { get; set; }
        public bool Active { get; set; }
        public int ProductsCount { get; set; } // Количество товаров в категории
    }
}
