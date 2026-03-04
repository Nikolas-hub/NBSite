using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class SubmitOrderVM
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Комментарий к заказу")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "Выберите способ доставки")]
        [Display(Name = "Способ доставки")]
        public int DeliveryId { get; set; }

        [Required(ErrorMessage = "Выберите способ оплаты")]
        [Display(Name = "Способ оплаты")]
        public int PaymentId { get; set; }

        [Display(Name = "Город")]
        public int? CityId { get; set; }

        [Display(Name = "Промокод")]
        public string? CouponCode { get; set; }
    }
}
