using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class UserProfileViewModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Логин")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Компания")]
        [StringLength(255, ErrorMessage = "Название компании не должно превышать 255 символов")]
        public string? Company { get; set; }

        [Display(Name = "Должность")]
        [StringLength(255, ErrorMessage = "Должность не должна превышать 255 символов")]
        public string? CompanyPost { get; set; }

        [Display(Name = "ФИО")]
        [StringLength(255, ErrorMessage = "ФИО не должно превышать 255 символов")]
        public string? Fio { get; set; }

        [Display(Name = "Город")]
        public long? CityId { get; set; }

        [Display(Name = "Видит цены")]
        public bool PricesVisible { get; set; }
    }
}

