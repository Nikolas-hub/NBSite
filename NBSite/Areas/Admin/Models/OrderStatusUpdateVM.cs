using System.ComponentModel.DataAnnotations;

namespace NBSite.Areas.Admin.Models
{
    public class OrderStatusUpdateVM
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public int NewStatus { get; set; }
    }
}
