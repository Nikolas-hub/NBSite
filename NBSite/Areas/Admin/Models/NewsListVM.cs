namespace NBSite.Areas.Admin.Models
{
    public class NewsListVM
    {
        public List<NewsListItemVM> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
    }
}
