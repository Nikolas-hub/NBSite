namespace NBSite.Areas.Admin.Models
{
    public class DiscountListItemVM
    {
        public long Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int DiscountType { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? Priority { get; set; }
    }
}
