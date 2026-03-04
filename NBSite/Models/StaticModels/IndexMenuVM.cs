using Domain.Entities;

namespace NBSite.Models.StaticModels
{
    public static class IndexMenuVM
    {
        private static List<CatalogCategory>? _categories;

        public static List<CatalogCategory> Categories
        {
            get => _categories ?? new List<CatalogCategory>();
            set => _categories = value;
        }

        // Метод для получения вложенных категорий
        public static List<CatalogCategory> GetRootCategories()
        {
            return Categories
                .Where(c => string.IsNullOrEmpty(c.Parent))
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .ToList();
        }

        // Метод для получения всех дочерних категорий
        public static List<CatalogCategory> GetChildCategories(string parentCode)
        {
            return Categories
                .Where(c => c.Parent == parentCode)
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .ToList();
        }

        // Метод для поиска категории по alias
        public static CatalogCategory? GetCategoryByAlias(string alias)
        {
            return Categories.FirstOrDefault(c => c.Alias == alias);
        }

        // Метод для получения цепочки хлебных крошек
        public static List<CatalogCategory> GetBreadcrumbs(string categoryAlias)
        {
            var breadcrumbs = new List<CatalogCategory>();
            var currentCategory = GetCategoryByAlias(categoryAlias);

            while (currentCategory != null)
            {
                breadcrumbs.Insert(0, currentCategory);
                currentCategory = Categories.FirstOrDefault(c => c.Code == currentCategory.Parent);
            }

            return breadcrumbs;
        }
    }
}
