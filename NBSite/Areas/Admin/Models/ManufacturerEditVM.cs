using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class ManufacturerEditVM
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Display(Name = "Алиас (URL)")]
        public string? Alias { get; set; }

        [Display(Name = "Код")]
        public string? Code { get; set; }

        [Display(Name = "Код страны")]
        public string? CountryCode { get; set; }

        [Display(Name = "Изображение")]
        public string? Image { get; set; }

        // Для загрузки нового изображения
        [Display(Name = "Загрузить новое изображение")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Сортировка")]
        public short Sort { get; set; }

        [Display(Name = "Активен")]
        public bool Active { get; set; }

        [Display(Name = "Краткое описание")]
        public string? Introtext { get; set; }

        [Display(Name = "Полное описание")]
        public string? Content { get; set; }

        [Display(Name = "Meta Title")]
        public string? MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [Display(Name = "Meta Keywords")]
        public string? MetaKeywords { get; set; }
    }
}
