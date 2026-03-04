using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class ProductEditVM
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Display(Name = "Алиас (URL)")]
        public string? Alias { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Display(Name = "Цена")]
        public double Price { get; set; }

        [Display(Name = "Старая цена")]
        public double OldPrice { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Кратность")]
        public int Multiplicity { get; set; }

        [Display(Name = "Краткое описание")]
        public string? Introtext { get; set; }

        [Display(Name = "Полное описание")]
        public string? Content { get; set; }

        [Display(Name = "Категория")]
        public long? CategoryId { get; set; }

        [Display(Name = "Производитель")]
        public long? ManufacturerId { get; set; }

        [Display(Name = "Текущее изображение")]
        public string? Image { get; set; }

        [Display(Name = "Новинка")]
        public bool New { get; set; }

        [Display(Name = "Популярный")]
        public bool Popular { get; set; }

        [Display(Name = "Активен")]
        public bool Active { get; set; }

        [Display(Name = "С браком")]
        public bool HasReject { get; set; }

        [Display(Name = "Объём")]
        public double Volume { get; set; }

        [Display(Name = "Вес")]
        public double Weight { get; set; }

        [Display(Name = "Штрих-код EAN13")]
        public string? Ean13 { get; set; }

        [Display(Name = "Срок годности")]
        public DateOnly? ExpirationDate { get; set; }
    }
}
