using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBSite.Areas.Admin.Models;

namespace NBSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class ManufacturersController : Controller
    {
        private readonly NbshopContext _db;
        private readonly IWebHostEnvironment _env;

        public ManufacturersController(NbshopContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Admin/Manufacturers
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;

            var query = _db.CatalogManufacturers.Where(m => m.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.Code.Contains(search));
            }

            var total = await query.CountAsync();

            var manufacturers = await query
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = manufacturers.Select(m => new ManufacturerListItemVM
            {
                Id = m.Id,
                Name = m.Name,
                Alias = m.Alias,
                Code = m.Code,
                CountryCode = m.CountryCode,
                Image = m.Image,
                Sort = m.Sort,
                Active = m.Active,
                ProductsCount = _db.CatalogProducts.Count(p => p.ManufacturerId == m.Id && p.Active)
            }).ToList();

            var model = new ManufacturerListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        // GET: Admin/Manufacturers/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var manufacturer = await _db.CatalogManufacturers.FindAsync(id);
            if (manufacturer == null)
                return NotFound();

            var model = new ManufacturerEditVM
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name,
                Alias = manufacturer.Alias,
                Code = manufacturer.Code,
                CountryCode = manufacturer.CountryCode,
                Image = manufacturer.Image,
                Sort = manufacturer.Sort,
                Active = manufacturer.Active,
                Introtext = manufacturer.Introtext,
                Content = manufacturer.Content,
                MetaTitle = manufacturer.MetaTitle,
                MetaDescription = manufacturer.MetaDescription,
                MetaKeywords = manufacturer.MetaKeywords
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManufacturerEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var manufacturer = await _db.CatalogManufacturers.FindAsync(model.Id);
            if (manufacturer == null)
                return NotFound();

            // Проверка уникальности Code (если изменился)
            if (model.Code != manufacturer.Code &&
                await _db.CatalogManufacturers.AnyAsync(m => m.Code == model.Code))
            {
                ModelState.AddModelError("Code", "Производитель с таким кодом уже существует");
                return View(model);
            }

            manufacturer.Name = model.Name!;
            manufacturer.Alias = model.Alias!;
            manufacturer.Code = model.Code!;
            manufacturer.CountryCode = model.CountryCode;
            manufacturer.Sort = model.Sort;
            manufacturer.Active = model.Active;
            manufacturer.Introtext = model.Introtext;
            manufacturer.Content = model.Content;
            manufacturer.MetaTitle = model.MetaTitle;
            manufacturer.MetaDescription = model.MetaDescription;
            manufacturer.MetaKeywords = model.MetaKeywords;

            // Обработка загрузки изображения
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "manufacturers");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                manufacturer.Image = "manufacturers/" + uniqueFileName;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Производитель успешно обновлён";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Manufacturers/Create
        public IActionResult Create()
        {
            return View(new ManufacturerEditVM { Active = true, Sort = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManufacturerEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверка уникальности Code
            if (await _db.CatalogManufacturers.AnyAsync(m => m.Code == model.Code))
            {
                ModelState.AddModelError("Code", "Производитель с таким кодом уже существует");
                return View(model);
            }

            var manufacturer = new CatalogManufacturer
            {
                Name = model.Name!,
                Alias = model.Alias!,
                Code = model.Code!,
                CountryCode = model.CountryCode,
                Sort = model.Sort,
                Active = model.Active,
                Introtext = model.Introtext,
                Content = model.Content,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords
            };

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "manufacturers");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                manufacturer.Image = "manufacturers/" + uniqueFileName;
            }

            _db.CatalogManufacturers.Add(manufacturer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Производитель успешно добавлен";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var manufacturer = await _db.CatalogManufacturers.FindAsync(id);
            if (manufacturer == null)
                return NotFound();

            // Проверка на наличие товаров
            if (await _db.CatalogProducts.AnyAsync(p => p.ManufacturerId == id))
            {
                TempData["Error"] = "Нельзя удалить производителя, у которого есть товары";
                return RedirectToAction(nameof(Index));
            }

            _db.CatalogManufacturers.Remove(manufacturer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Производитель удалён";
            return RedirectToAction(nameof(Index));
        }
    }
}