using Domain.Entities;

namespace NBSite.Models.ViewComponents
{
    public class IndexVM
    {
        public List<NewsPreviewVM> LatestNews { get; set; } = new();
        public List<ProductPreviewVM> PopularProducts { get; set; } = new();
        public List<ProductPreviewVM> NewProducts { get; set; } = new();
    }
}
