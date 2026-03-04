namespace NBSite.Areas.Admin.Models
{
    public class OrderDetailsVM
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Status { get; set; }
        public int StatusValue { get; set; }
        public string? Reciever { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Comment { get; set; }

        public string? DeliveryName { get; set; }
        public long? DeliveryId { get; set; }

        public string? PaymentName { get; set; }
        public long? PaymentId { get; set; }

        public string? CityName { get; set; }
        public long? CityId { get; set; }

        public string? CouponCode { get; set; }

        public long? UserId { get; set; }
        public string? UserName { get; set; }

        public List<OrderProductItemVM>? Products { get; set; }
        public double Subtotal { get; set; }
        public double Total { get; set; }
    }

    public class OrderProductItemVM
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductAlias { get; set; }
        public double Price { get; set; }
        public long Quantity { get; set; }
        public double Total => Price * Quantity;
    }
}
