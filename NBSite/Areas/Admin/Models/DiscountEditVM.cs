using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class DiscountEditVM
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Выберите товар")]
        [Display(Name = "Товар")]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Выберите тип скидки")]
        [Display(Name = "Тип скидки")]
        public DiscountType DiscountType { get; set; }  // теперь enum

        [Required(ErrorMessage = "Укажите значение")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
        [Display(Name = "Значение")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Укажите дату начала")]
        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Укажите дату окончания")]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Приоритет")]
        public int? Priority { get; set; }
    }

    public enum DiscountType
    {
        [Display(Name = "Процент")]
        Percentage = 1,
        [Display(Name = "Фиксированная сумма")]
        FixedAmount = 2,
        [Display(Name = "Новая цена")]
        NewPrice = 3
    }
}
