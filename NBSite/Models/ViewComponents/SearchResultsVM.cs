namespace NBSite.Models.ViewComponents
{
    public class SearchResultsVM
    {
        public string Query { get; set; } = string.Empty;
        public List<ProductPreviewVM> Products { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
