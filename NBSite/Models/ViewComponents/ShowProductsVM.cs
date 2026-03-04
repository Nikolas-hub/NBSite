using Domain.Entities;

namespace NBSite.Models.ViewComponents
{
    public class ShowProductsVM
    {
        public CatalogCategory? Category { get; set; }
        public List<ProductPreviewVM> Products { get; set; } = new();
        public string? PriceFilter { get; set; }
        public string? SortOrder { get; set; }
        public string? Query { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<PriceRangeVM> PriceRanges { get; set; } = new();
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool? InStock { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public List<long> SelectedCategoryIds { get; set; } = new();
        public List<CategoryFilterDto> AllCategories { get; set; } = new();
        public List<CatalogCategory> CategoryPath { get; set; } = new();
    }
}
