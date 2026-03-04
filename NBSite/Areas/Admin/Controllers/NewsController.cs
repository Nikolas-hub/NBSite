using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBSite.Areas.Admin.Models;
using System.Text.RegularExpressions;

namespace NBSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class NewsController : Controller
    {
        private readonly NbshopContext _db;
        private readonly IWebHostEnvironment _env;

        public NewsController(NbshopContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Admin/News
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;
            var query = _db.ContentNews.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n => n.Name.Contains(search) || n.Id.ToString() == search);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.Date)
                .ThenByDescending(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewsListItemVM
                {
                    Id = n.Id,
                    Name = n.Name,
                    Date = n.Date,
                    Active = n.Active,
                    Image = n.Image,
                    Sort = n.Sort
                })
                .ToListAsync();

            var model = new NewsListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        // GET: Admin/News/Create
        public IActionResult Create()
        {
            return View(new NewsEditVM { Date = DateTime.Today });
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsEditVM model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Генерация алиаса, если не задан
                if (string.IsNullOrWhiteSpace(model.Alias))
                {
                    model.Alias = GenerateAlias(model.Name);
                }

                // Проверка уникальности алиаса
                if (await _db.ContentNews.AnyAsync(n => n.Alias == model.Alias))
                {
                    ModelState.AddModelError("Alias", "Такой алиас уже существует. Придумайте другой или оставьте поле пустым для автоматической генерации.");
                    return View(model);
                }

                var news = new ContentNews
                {
                    Name = model.Name,
                    Alias = model.Alias,
                    Introtext = model.Introtext,
                    Content = model.Content,
                    Date = DateOnly.FromDateTime(model.Date),
                    Active = model.Active,
                    Sort = model.Sort,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords
                };

                // Обработка изображения
                if (imageFile != null && imageFile.Length > 0)
                {
                    news.Image = await SaveImage(imageFile);
                }

                _db.ContentNews.Add(news);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Новость успешно создана";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/News/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var news = await _db.ContentNews.FindAsync(id);
            if (news == null)
                return NotFound();

            var model = new NewsEditVM
            {
                Id = news.Id,
                Name = news.Name,
                Alias = news.Alias,
                Introtext = news.Introtext,
                Content = news.Content,
                Date = news.Date.ToDateTime(TimeOnly.MinValue),
                Image = news.Image,
                MetaTitle = news.MetaTitle,
                MetaDescription = news.MetaDescription,
                MetaKeywords = news.MetaKeywords,
                Active = news.Active,
                Sort = news.Sort
            };

            return View(model);
        }

        // POST: Admin/News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, NewsEditVM model, IFormFile? imageFile)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var news = await _db.ContentNews.FindAsync(id);
                if (news == null)
                    return NotFound();

                // Генерация алиаса, если не задан
                if (string.IsNullOrWhiteSpace(model.Alias))
                {
                    model.Alias = GenerateAlias(model.Name);
                }

                // Проверка уникальности алиаса (исключая текущую новость)
                if (await _db.ContentNews.AnyAsync(n => n.Alias == model.Alias && n.Id != id))
                {
                    ModelState.AddModelError("Alias", "Такой алиас уже существует. Придумайте другой.");
                    return View(model);
                }

                news.Name = model.Name;
                news.Alias = model.Alias;
                news.Introtext = model.Introtext;
                news.Content = model.Content;
                news.Date = DateOnly.FromDateTime(model.Date);
                news.Active = model.Active;
                news.Sort = model.Sort;
                news.MetaTitle = model.MetaTitle;
                news.MetaDescription = model.MetaDescription;
                news.MetaKeywords = model.MetaKeywords;

                // Обработка нового изображения
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Удалить старое изображение, если нужно
                    if (!string.IsNullOrEmpty(news.Image))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, news.Image.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    news.Image = await SaveImage(imageFile);
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Новость обновлена";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Admin/News/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var news = await _db.ContentNews.FindAsync(id);
            if (news == null)
                return NotFound();

            // Удалить файл изображения
            if (!string.IsNullOrEmpty(news.Image))
            {
                var path = Path.Combine(_env.WebRootPath, news.Image.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _db.ContentNews.Remove(news);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Новость удалена";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для сохранения изображения
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "news");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return "news/" + uniqueFileName;
        }

        // Простая транслитерация кириллицы в латиницу для генерации алиаса
        private string GenerateAlias(string name)
        {
            var translit = new Dictionary<char, string>
            {
                {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
                {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
                {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
                {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
                {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"},
                {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
                {'э', "e"}, {'ю', "yu"}, {'я', "ya"},
                {'А', "a"}, {'Б', "b"}, {'В', "v"}, {'Г', "g"}, {'Д', "d"},
                {'Е', "e"}, {'Ё', "yo"}, {'Ж', "zh"}, {'З', "z"}, {'И', "i"},
                {'Й', "y"}, {'К', "k"}, {'Л', "l"}, {'М', "m"}, {'Н', "n"},
                {'О', "o"}, {'П', "p"}, {'Р', "r"}, {'С', "s"}, {'Т', "t"},
                {'У', "u"}, {'Ф', "f"}, {'Х', "h"}, {'Ц', "ts"}, {'Ч', "ch"},
                {'Ш', "sh"}, {'Щ', "sch"}, {'Ъ', ""}, {'Ы', "y"}, {'Ь', ""},
                {'Э', "e"}, {'Ю', "yu"}, {'Я', "ya"}
            };

            var result = string.Concat(name.Select(c =>
                translit.ContainsKey(c) ? translit[c] : c.ToString()
            ));

            // Заменяем пробелы и прочие символы на дефисы
            result = Regex.Replace(result, @"[^a-zA-Z0-9\s-]", "");
            result = Regex.Replace(result, @"\s+", "-");
            result = result.Trim('-');
            return result.ToLower();
        }
    }
}