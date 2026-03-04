namespace NBSite.Models.ViewComponents
{
    public class OrderHistoryItemVM
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public double Total { get; set; }
        public int ItemsCount { get; set; }
    }
}
