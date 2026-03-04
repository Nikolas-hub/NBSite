using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NBSite.Models;

public interface ICategoryMenuService
{
    Task<IEnumerable<CategoryMenuDto>> GetMenuCategoriesAsync();
}

public class CategoryMenuService : ICategoryMenuService
{
    private readonly NbshopContext _context;

    public CategoryMenuService(NbshopContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryMenuDto>> GetMenuCategoriesAsync()
    {
        // Загружаем все активные категории
        var categories = await _context.CatalogCategories
            .Where(c => c.Active)
            .Select(c => new CategoryMenuDto
            {
                Id = c.Id,
                Name = c.Name,
                Alias = c.Alias,
                Code = c.Code,
                ParentCode = c.Parent,
                Active = c.Active
            })
            .ToListAsync();

        // Строим иерархию
        var lookup = categories.ToLookup(c => c.ParentCode);
        foreach (var cat in categories)
        {
            cat.Children = lookup[cat.Code].ToList();
        }

        // Возвращаем только корневые категории (ParentCode == null)
        return categories.Where(c => c.ParentCode == null).ToList();
    }
}