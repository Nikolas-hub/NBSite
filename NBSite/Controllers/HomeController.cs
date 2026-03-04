using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBSite.Models;
using NBSite.Models.ViewComponents;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;


namespace NBSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly NbshopContext _db;
        private readonly IConfiguration _configuration;

        public HomeController(NbshopContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _configuration = configuration;
        }

        // Метод загрузки категорий в ViewBag
        private async Task LoadMenuCategoriesAsync()
        {
            var allCategories = await _db.CatalogCategories
                .Where(c => c.Active)
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var categoriesByParent = allCategories
    .Where(c => !string.IsNullOrEmpty(c.Parent))
    .GroupBy(c => c.Parent)
    .ToDictionary(g => g.Key!, g => g.ToList()); // добавлен !

            var rootCategories = allCategories
                .Where(c => string.IsNullOrEmpty(c.Parent))
                .Select(c => new CategoryMenuDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Alias = c.Alias,
                    Code = c.Code,
                    Children = categoriesByParent.ContainsKey(c.Code!)
                        ? categoriesByParent[c.Code!].Select(ch => new CategoryMenuDto
                        {
                            Id = ch.Id,
                            Name = ch.Name,
                            Alias = ch.Alias,
                            Code = ch.Code,
                            Children = new List<CategoryMenuDto>() // если нужна глубокая вложенность – доработать
                        }).ToList()
                        : new List<CategoryMenuDto>()
                })
                .ToList();

            ViewBag.MenuCategories = rootCategories;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadMenuCategoriesAsync();

            // Корзина
            if (User.Identity!.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId > 0)
                {
                    var cart = await _db.CatalogOrders
                        .Include(o => o.CatalogOrderproducts)
                        .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == (long)OrderStatus.Cart);

                    ViewData["AmountBasket"] = cart?.CatalogOrderproducts.Sum(p => p.Quantity) ?? 0;
                    ViewData["CartId"] = cart?.Id;
                }
            }
            else
            {
                ViewData["AmountBasket"] = 0;
            }

            // Новости
            var latestNews = await _db.ContentNews
                .Where(n => n.Active == true)
                .OrderByDescending(n => n.Date)
                .ThenByDescending(n => n.Id) // CreatedAt заменён на Id
                .Take(5)
                .Select(n => new NewsPreviewVM
                {
                    Id = n.Id,
                    Title = n.Name,
                    Alias = n.Alias,
                    Date = n.Date,
                    IntroText = n.Introtext,
                    Image = n.Image
                })
                .ToListAsync();

            // Популярные товары
            var popularProducts = await _db.CatalogProducts
                .Where(p => p.Active == true && p.Popular == true && p.Quantity > 0)  // добавлено условие наличия
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .OrderBy(p => p.Sort)
                .ThenByDescending(p => p.Id)
                .Take(8)
                .Select(p => new ProductPreviewVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Alias = p.Alias,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    Image = p.Image,
                    IsNew = p.New,
                    IsPopular = p.Popular,
                    CategoryName = p.Category!.Name,
                    CategoryAlias = p.Category.Alias,
                    Quantity = p.Quantity,
                    ManufacturerName = p.Manufacturer!.Name
                })
                .ToListAsync();

            // Новые товары
            var newProducts = await _db.CatalogProducts
                .Where(p => p.Active == true && p.New == true && p.Quantity > 0)      // добавлено условие наличия
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .Select(p => new ProductPreviewVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Alias = p.Alias,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    Image = p.Image,
                    IsNew = p.New,
                    IsPopular = p.Popular,
                    CategoryName = p.Category!.Name,
                    CategoryAlias = p.Category.Alias,
                    Quantity = p.Quantity,
                    ManufacturerName = p.Manufacturer!.Name
                })
                .ToListAsync();

            var model = new IndexVM
            {
                LatestNews = latestNews,
                PopularProducts = popularProducts,
                NewProducts = newProducts
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delivery()
        {
            await LoadMenuCategoriesAsync(); // подгружаем категории для меню
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Contacts()
        {
            await LoadMenuCategoriesAsync(); // это остаётся для меню

            var model = new ContactsViewModel();

            // Ищем группу "managers"
            var managerGroup = await _db.AuthGroups
                .FirstOrDefaultAsync(g => g.Name == "managers");

            if (managerGroup != null)
            {
                // Получаем идентификаторы пользователей, входящих в группу
                var managerUserIds = await _db.AuthUserGroups
                    .Where(ug => ug.GroupId == managerGroup.Id)
                    .Select(ug => ug.UserId)
                    .ToListAsync();

                // Загружаем пользователей с их профилями (нужно включить навигационное свойство)
                var managers = await _db.AuthUsers
                    .Include(u => u.AccountsProfile)
                    .Where(u => managerUserIds.Contains(u.Id))
                    .ToListAsync();

                // Преобразуем в ManagerViewModel
                model.Managers = managers.Select(u => new ManagerViewModel
                {
                    FullName = u.AccountsProfile?.Fio ??
                               $"{u.FirstName} {u.LastName}".Trim() ??
                               u.Username,
                    Phone = u.AccountsProfile?.Phone,
                    Email = u.Email
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Callback([FromForm] ContactsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { status = false, message = "Заполните обязательные поля", errors });
            }

            try
            {
                // Получаем email менеджеров (можно из базы или из конфигурации)
                var managerEmails = await GetManagerEmailsAsync();

                string subject = "Заказ звонка с сайта";
                string body = $"Телефон: {model.Phone}\nИмя: {model.Name}";

                // Настройки SMTP из конфигурации
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:Port"]!);
                var smtpUser = _configuration["Email:Username"];
                var smtpPass = _configuration["Email:Password"];
                var fromEmail = _configuration["Email:From"];

                using var smtp = new SmtpClient(smtpServer, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                foreach (var email in managerEmails)
                {
                    mailMessage.To.Add(email);
                }

                await smtp.SendMailAsync(mailMessage);

                return Json(new { status = true, message = "Заявка отправлена. Ожидайте звонка" });
            }
            catch (Exception)
            {
                // Логирование ошибки (можно использовать ILogger)
                return Json(new { status = false, message = "Ошибка при отправке. Попробуйте позже." });
            }
        }

        // Вспомогательный метод для получения email менеджеров
        private async Task<List<string>> GetManagerEmailsAsync()
        {
            var managerGroup = await _db.AuthGroups
                .FirstOrDefaultAsync(g => g.Name == "managers");
            if (managerGroup == null)
                return new List<string>();

            var emails = await _db.AuthUserGroups
                .Where(ug => ug.GroupId == managerGroup.Id)
                .Join(_db.AuthUsers, ug => ug.UserId, u => u.Id, (ug, u) => u.Email)
                .Where(email => !string.IsNullOrEmpty(email))
                .ToListAsync();

            return emails;
        }

        [HttpGet]
        public async Task<IActionResult> HowToBuy()
        {
            await LoadMenuCategoriesAsync(); 
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Terms()
        {
            await LoadMenuCategoriesAsync(); 
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            await LoadMenuCategoriesAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Page(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return NotFound();

            await LoadMenuCategoriesAsync();

            var page = await _db.ContentPages
                .FirstOrDefaultAsync(p => p.Alias == alias && p.Active == true);

            return page == null ? NotFound() : View(page);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return View(new SearchResultsVM { Query = query });

            await LoadMenuCategoriesAsync();

            // Логируем запрос
            var searchLog = new SearchSearchlogentry
            {
                Text = query,
                CreatedAt = DateTime.UtcNow
            };
            await _db.SearchSearchlogentries.AddAsync(searchLog);
            await _db.SaveChangesAsync();

            var normalizedQuery = query.ToLower();

            var productsQuery = _db.CatalogProducts
                .Where(p => p.Active == true &&
                    (p.Name.ToLower().Contains(normalizedQuery) ||
                     p.Introtext != null && p.Introtext.ToLower().Contains(normalizedQuery) ||
                     p.Content != null && p.Content.ToLower().Contains(normalizedQuery) ||
                     p.SearchKeywords != null && p.SearchKeywords.ToLower().Contains(normalizedQuery)))
                .Include(p => p.Category)
                .Include(p => p.Manufacturer);

            var totalCount = await productsQuery.CountAsync();

            var products = await productsQuery
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductPreviewVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Alias = p.Alias,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    Image = p.Image,
                    IntroText = p.Introtext,
                    CategoryName = p.Category!.Name,
                    CategoryAlias = p.Category.Alias,
                    ManufacturerName = p.Manufacturer!.Name
                })
                .ToListAsync();

            var model = new SearchResultsVM
            {
                Query = query,
                Products = products,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { count = 0 });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId == 0)
                return Json(new { count = 0 });

            var cart = await _db.CatalogOrders
                .Include(o => o.CatalogOrderproducts)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == (long)OrderStatus.Cart);

            var count = cart?.CatalogOrderproducts.Sum(p => p.Quantity) ?? 0;
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuCategories()
        {
            var allCategories = await _db.CatalogCategories
                .Where(c => c.Active)
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var categoriesByParent = allCategories
    .Where(c => !string.IsNullOrEmpty(c.Parent))
    .GroupBy(c => c.Parent)
    .ToDictionary(g => g.Key!, g => g.ToList()); // добавлен !

            var rootCategories = allCategories
                .Where(c => string.IsNullOrEmpty(c.Parent))
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Alias,
                    c.Code,
                    Children = categoriesByParent.ContainsKey(c.Code!)
                        ? categoriesByParent[c.Code!].Select(ch => new { ch.Id, ch.Name, ch.Alias, ch.Code })
                                           .Cast<object>().ToList()
                        : new List<object>()
                })
                .ToList();

            return Json(rootCategories);
        }
    }
}