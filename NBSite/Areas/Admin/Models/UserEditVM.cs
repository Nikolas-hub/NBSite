using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class UserEditVM
    {
        public long Id { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Сотрудник (доступ в админку)")]
        public bool IsStaff { get; set; }

        [Display(Name = "Суперпользователь")]
        public bool IsSuperuser { get; set; }
    }
}
