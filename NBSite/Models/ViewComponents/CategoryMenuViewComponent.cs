using Microsoft.AspNetCore.Mvc;

namespace NBSite.Models.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryMenuService _categoryMenuService;

        public CategoryMenuViewComponent(ICategoryMenuService categoryMenuService)
        {
            _categoryMenuService = categoryMenuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryMenuService.GetMenuCategoriesAsync();
            return View(categories);
        }
    }
}
