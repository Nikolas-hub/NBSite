using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Текущий пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
