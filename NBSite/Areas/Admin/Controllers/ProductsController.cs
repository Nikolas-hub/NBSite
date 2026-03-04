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
    public class ProductsController : Controller
    {
        private readonly NbshopContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(NbshopContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;
            var query = _db.CatalogProducts
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Where(p => p.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Id.ToString() == search);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListItemVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryName = p.Category!.Name,
                    ManufacturerName = p.Manufacturer != null ? p.Manufacturer.Name : "",
                    Image = p.Image
                })
                .ToListAsync();

            var model = new ProductListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var product = await _db.CatalogProducts
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var model = new ProductEditVM
            {
                Id = product.Id,
                Name = product.Name,
                Alias = product.Alias,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Quantity = product.Quantity,
                Multiplicity = product.Multiplicity,
                Introtext = product.Introtext,
                Content = product.Content,
                CategoryId = product.CategoryId,
                ManufacturerId = product.ManufacturerId,
                Image = product.Image,
                New = product.New,
                Popular = product.Popular,
                Active = product.Active,
                HasReject = product.HasReject,
                Volume = product.Volume,
                Weight = product.Weight,
                Ean13 = product.Ean13,
                ExpirationDate = product.ExpirationDate
            };

            ViewBag.Categories = new SelectList(await _db.CatalogCategories.Where(c => c.Active).ToListAsync(), "Id", "Name");
            ViewBag.Manufacturers = new SelectList(await _db.CatalogManufacturers.Where(m => m.Active).ToListAsync(), "Id", "Name");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditVM model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.CatalogCategories.Where(c => c.Active).ToListAsync(), "Id", "Name");
                ViewBag.Manufacturers = new SelectList(await _db.CatalogManufacturers.Where(m => m.Active).ToListAsync(), "Id", "Name");
                return View(model);
            }

            var product = await _db.CatalogProducts.FindAsync(model.Id);
            if (product == null)
                return NotFound();

            // Обновляем поля
            product.Name = model.Name!;
            product.Alias = model.Alias!;
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            product.Quantity = model.Quantity;
            product.Multiplicity = model.Multiplicity;
            product.Introtext = model.Introtext;
            product.Content = model.Content;
            product.CategoryId = model.CategoryId;
            product.ManufacturerId = model.ManufacturerId;
            product.New = model.New;
            product.Popular = model.Popular;
            product.Active = model.Active;
            product.HasReject = model.HasReject;
            product.Volume = model.Volume;
            product.Weight = model.Weight;
            product.Ean13 = model.Ean13;
            product.ExpirationDate = model.ExpirationDate;

            // Обработка загрузки нового изображения
            if (imageFile != null && imageFile.Length > 0)
            {
                // Папка для сохранения изображений
                var uploadsFolder = Path.Combine(_env.WebRootPath, "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Получаем расширение файла
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                // Очищаем алиас от недопустимых символов для имени файла
                string safeAlias = SanitizeFileName(product.Alias);
                if (string.IsNullOrWhiteSpace(safeAlias))
                {
                    // Если алиас пустой или после очистки ничего не осталось, используем ID товара
                    safeAlias = $"product-{product.Id}";
                }

                // Формируем имя файла: Алиас + расширение
                string fileName = safeAlias + fileExtension;
                string relativePath = Path.Combine("products", fileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);

                // Удаляем старое изображение, если оно существует и отличается от нового
                if (!string.IsNullOrEmpty(product.Image))
                {
                    string oldFullPath = Path.Combine(_env.WebRootPath, product.Image.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFullPath) && oldFullPath != fullPath)
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }

                // Сохраняем новый файл (перезаписываем, если уже существует)
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Обновляем путь к изображению в базе данных
                product.Image = relativePath;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Товар успешно обновлён";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Очищает строку от символов, недопустимых в именах файлов.
        /// Заменяет пробелы и небуквенно-цифровые символы на дефисы.
        /// </summary>
        private string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Удаляем символы, не являющиеся буквами, цифрами, дефисами или подчёркиваниями
            var sanitized = System.Text.RegularExpressions.Regex.Replace(input, @"[^\w\-]", "-");
            // Заменяем множественные дефисы на один
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"-+", "-");
            // Убираем дефисы в начале и конце
            return sanitized.Trim('-');
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.CatalogCategories.Where(c => c.Active).ToListAsync(), "Id", "Name");
            ViewBag.Manufacturers = new SelectList(await _db.CatalogManufacturers.Where(m => m.Active).ToListAsync(), "Id", "Name");
            return View(new ProductEditVM { Active = true, Multiplicity = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditVM model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.CatalogCategories.Where(c => c.Active).ToListAsync(), "Id", "Name");
                ViewBag.Manufacturers = new SelectList(await _db.CatalogManufacturers.Where(m => m.Active).ToListAsync(), "Id", "Name");
                return View(model);
            }

            var product = new CatalogProduct
            {
                Name = model.Name!,
                Alias = model.Alias!,
                Price = model.Price,
                OldPrice = model.OldPrice,
                Quantity = model.Quantity,
                Multiplicity = model.Multiplicity,
                Introtext = model.Introtext,
                Content = model.Content,
                CategoryId = model.CategoryId,
                ManufacturerId = model.ManufacturerId,
                New = model.New,
                Popular = model.Popular,
                Active = model.Active,
                HasReject = model.HasReject,
                Volume = model.Volume,
                Weight = model.Weight,
                Ean13 = model.Ean13,
                ExpirationDate = model.ExpirationDate
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.Image = "products/" + uniqueFileName;
            }

            _db.CatalogProducts.Add(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Товар успешно добавлен";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var product = await _db.CatalogProducts.FindAsync(id);
            if (product == null)
                return NotFound();

            // Лучше мягкое удаление или проверка на связанные заказы
            _db.CatalogProducts.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Товар удалён";
            return RedirectToAction(nameof(Index));
        }
    }
}
