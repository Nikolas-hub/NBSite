namespace NBSite.Areas.Admin.Models
{
    public class NewsListItemVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public bool Active { get; set; }
        public string? Image { get; set; }
        public int Sort { get; set; }
    }
}
