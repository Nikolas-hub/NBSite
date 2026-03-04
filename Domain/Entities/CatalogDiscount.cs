
namespace Domain.Entities
{
    public partial class CatalogDiscount
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public int DiscountType { get; set; }      // 1 – процент, 2 – фиксированная сумма, 3 – новая цена
        public decimal Value { get; set; }         // значение скидки (процент, сумма или цена)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? Priority { get; set; }         // приоритет при наложении скидок (чем меньше число, тем выше приоритет)

        // Навигационное свойство к товару
        public virtual CatalogProduct? Product { get; set; }
    }
}
