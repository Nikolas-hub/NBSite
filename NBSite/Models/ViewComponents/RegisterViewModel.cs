using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Display(Name = "Логин (необязательно)")]
        public string? Username { get; set; }

        [Phone(ErrorMessage = "Некорректный телефон")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        [Display(Name = "Пароль")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public required string ConfirmPassword { get; set; }

        // новые поля
        [Display(Name = "Компания")]
        public string? Company { get; set; }

        [Display(Name = "Город")]
        public int? CityId { get; set; }
    }
}
