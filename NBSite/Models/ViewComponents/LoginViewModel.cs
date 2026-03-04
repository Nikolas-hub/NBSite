using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email или логин обязателен")]
        [Display(Name = "Email или логин")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; } = true;
    }
}
