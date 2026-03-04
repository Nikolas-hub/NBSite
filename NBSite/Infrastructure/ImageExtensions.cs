using Domain.Entities;

namespace NBSite.Infrastructure
{
    public static class ImageExtensions
    {
        public static string GetThumbnailUrl(this string? imageUrl, int width, int height)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return "/img/no-image.png";

            // Если изображение уже содержит параметры запроса
            if (imageUrl.Contains('?'))
            {
                return $"{imageUrl}&size={width}x{height}";
            }

            return $"{imageUrl}?size={width}x{height}";
        }

        public static string GetProductGridThumbnail(this CatalogProduct product)
        {
            return product.Image.GetThumbnailUrl(300, 300);
        }

        public static string GetProductListThumbnail(this CatalogProduct product)
        {
            return product.Image.GetThumbnailUrl(100, 100);
        }

        public static string GetNewsThumbnail(this ContentNews news)
        {
            return news.Image.GetThumbnailUrl(600, 400);
        }

        public static string GetManufacturerThumbnail(this CatalogManufacturer manufacturer)
        {
            return manufacturer.Image.GetThumbnailUrl(150, 150);
        }

        public static string GetProfileThumbnail(this AccountsProfile profile)
        {
            return profile.Image.GetThumbnailUrl(200, 200);
        }
    }
}
