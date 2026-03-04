namespace NBSite.Areas.Admin.Models
{
    public class OrderListVM
    {
        public List<OrderListItemVM>? Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchQuery { get; set; }
        public int? StatusFilter { get; set; }
    }
}
