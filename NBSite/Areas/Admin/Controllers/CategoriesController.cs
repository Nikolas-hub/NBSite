using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBSite.Areas.Admin.Models;

namespace NBSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class CategoriesController : Controller
    {
        private readonly NbshopContext _db;

        public CategoriesController(NbshopContext db)
        {
            _db = db;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;

            var query = _db.CatalogCategories.Where(c => c.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Code!.Contains(search));
            }

            var total = await query.CountAsync();

            // Загружаем все категории, чтобы потом сопоставить родительские имена
            var categories = await query
                .OrderBy(c => c.Parent ?? "") // сначала корневые
                .ThenBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Для получения имён родителей составим словарь код->название
            var allCategories = await _db.CatalogCategories
                .Where(c => c.Code != null)  // исключаем возможные null
                .ToDictionaryAsync(c => c.Code!, c => c.Name ?? "");

            var items = categories.Select(c => new CategoryListItemVM
            {
                Id = c.Id,
                Name = c.Name,
                Alias = c.Alias,
                Code = c.Code,
                ParentName = !string.IsNullOrEmpty(c.Parent) && allCategories.ContainsKey(c.Parent)
                    ? allCategories[c.Parent]
                    : "",
                Sort = c.Sort,
                Active = c.Active,
                ProductsCount = _db.CatalogProducts.Count(p => p.CategoryId == c.Id && p.Active)
            }).ToList();

            var model = new CategoryListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var category = await _db.CatalogCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            var model = new CategoryEditVM
            {
                Id = category.Id,
                Name = category.Name,
                Alias = category.Alias,
                Code = category.Code,
                ParentCode = category.Parent,
                Sort = category.Sort,
                Active = category.Active,
                Introtext = category.Introtext,
                Content = category.Content,
                MetaTitle = category.MetaTitle,
                MetaDescription = category.MetaDescription,
                MetaKeywords = category.MetaKeywords
            };

            await LoadParentCategories(category.Code!);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryEditVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories(model.Code!);
                return View(model);
            }

            var category = await _db.CatalogCategories.FindAsync(model.Id);
            if (category == null)
                return NotFound();

            // Проверка на цикл: родитель не может быть потомком самого себя (упрощённо запрещаем самому себе быть родителем)
            if (!string.IsNullOrEmpty(model.ParentCode) && model.ParentCode == category.Code)
            {
                ModelState.AddModelError("ParentCode", "Категория не может быть родителем самой себя");
                await LoadParentCategories(category.Code);
                return View(model);
            }

            category.Name = model.Name!;
            category.Alias = model.Alias!;
            category.Code = model.Code;
            category.Parent = model.ParentCode;
            category.Sort = model.Sort!;
            category.Active = model.Active;
            category.Introtext = model.Introtext;
            category.Content = model.Content;
            category.MetaTitle = model.MetaTitle;
            category.MetaDescription = model.MetaDescription;
            category.MetaKeywords = model.MetaKeywords;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Категория успешно обновлена";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            await LoadParentCategories();
            return View(new CategoryEditVM { Active = true, Sort = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryEditVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories(model.Code!);
                return View(model);
            }

            // Проверка уникальности Code
            if (await _db.CatalogCategories.AnyAsync(c => c.Code == model.Code))
            {
                ModelState.AddModelError("Code", "Категория с таким кодом уже существует");
                await LoadParentCategories(model.Code!);
                return View(model);
            }

            var category = new CatalogCategory
            {
                Name = model.Name!,
                Alias = model.Alias!,
                Code = model.Code,
                Parent = model.ParentCode,
                Sort = model.Sort!,
                Active = model.Active,
                Introtext = model.Introtext,
                Content = model.Content,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords
            };

            _db.CatalogCategories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Категория успешно добавлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var category = await _db.CatalogCategories.FindAsync(id);

            if (category == null)
                return NotFound();

            // Проверка на наличие дочерних категорий
            bool hasChildren = await _db.CatalogCategories.AnyAsync(c => c.Parent == category.Code);
            if (hasChildren)
            {
                TempData["Error"] = "Нельзя удалить категорию, у которой есть подкатегории";
                return RedirectToAction(nameof(Index));
            }

            // Проверка на наличие товаров
            if (await _db.CatalogProducts.AnyAsync(p => p.CategoryId == id))
            {
                TempData["Error"] = "Нельзя удалить категорию, содержащую товары";
                return RedirectToAction(nameof(Index));
            }

            _db.CatalogCategories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Категория удалена";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadParentCategories(string? excludeCode = null)
        {
            var query = _db.CatalogCategories.Where(c => c.Active);
            if (!string.IsNullOrEmpty(excludeCode))
            {
                query = query.Where(c => c.Code != excludeCode);
            }
            var parents = await query
                .Where(c => c.Code != null)
                .OrderBy(c => c.Parent ?? "")
                .ThenBy(c => c.Name)
                .Select(c => new { Code = c.Code!, Name = c.Name ?? "" })
                .ToListAsync();
            ViewBag.Parents = new SelectList(parents, "Code", "Name");
        }
    }
}