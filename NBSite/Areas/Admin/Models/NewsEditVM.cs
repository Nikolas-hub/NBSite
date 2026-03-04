using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class NewsEditVM
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Введите название новости")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Алиас (если оставить пустым, сгенерируется автоматически)")]
        public string? Alias { get; set; }

        [Display(Name = "Краткое описание")]
        public string? Introtext { get; set; }

        [Display(Name = "Полный текст")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Укажите дату публикации")]
        [Display(Name = "Дата публикации")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Изображение")]
        public string? Image { get; set; }

        [Display(Name = "Meta Title")]
        [MaxLength(80, ErrorMessage = "Максимум 80 символов")]
        public string? MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
        public string? MetaDescription { get; set; }

        [Display(Name = "Meta Keywords")]
        [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
        public string? MetaKeywords { get; set; }

        [Display(Name = "Активна")]
        public bool Active { get; set; } = true;

        [Display(Name = "Порядок сортировки")]
        public short Sort { get; set; }
    }
}
