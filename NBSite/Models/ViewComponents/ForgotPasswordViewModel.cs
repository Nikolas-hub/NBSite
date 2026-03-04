using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
