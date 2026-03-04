using System.ComponentModel.DataAnnotations;

namespace NBSite.Models.ViewComponents
{
    public class AddToBasketVM
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        [Range(1, 999)]
        public long Quantity { get; set; } = 1;

        public long? CategoryId { get; set; }
    }
}

