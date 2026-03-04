using Domain.Entities;

namespace NBSite.Models.ViewComponents
{
    public class ShowBasketVM
    {
        public CatalogOrder Cart { get; set; } = null!;
        public List<CatalogDelivery> Deliveries { get; set; } = new();
        public List<CatalogPayment> Payments { get; set; } = new();
        public List<ReferencesCity> Cities { get; set; } = new();
    }
}
