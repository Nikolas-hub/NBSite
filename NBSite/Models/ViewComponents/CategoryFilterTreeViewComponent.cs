using Microsoft.AspNetCore.Mvc;

namespace NBSite.Models.ViewComponents
{
    public class CategoryFilterTreeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<CategoryFilterDto> categories)
        {
            return View(categories);
        }
    }
}
