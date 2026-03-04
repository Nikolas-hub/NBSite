namespace NBSite.Areas.Admin.Models
{
    public class OrderListItemVM
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Status { get; set; }
        public int StatusValue { get; set; }
        public string? Reciever { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public double Total { get; set; }
        public int ItemsCount { get; set; }
        public string? DeliveryName { get; set; }
        public string? PaymentName { get; set; }
        public string? CityName { get; set; }
        public string? UserName { get; set; }
    }
}
