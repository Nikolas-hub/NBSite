using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NBSite.Controllers
{
    public class NewsController : Controller
    {
        private readonly NbshopContext _context;  

        public NewsController(NbshopContext context)
        {
            _context = context;
        }

        // GET: /News/
        public async Task<IActionResult> Index()
        {
            var news = await _context.ContentNews
                .Where(n => n.Active)
                .OrderByDescending(n => n.Date)
                .ThenByDescending(n => n.Id)       
                .ToListAsync();

            return View(news);
        }

        // GET: /News/Details/5/alias
        public async Task<IActionResult> Details(int id, string alias)
        {
            var news = await _context.ContentNews
                .FirstOrDefaultAsync(n => n.Id == id && n.Active);

            if (news == null)
                return NotFound();

            // SEO: если alias не совпадает с текущим, делаем редирект на правильный URL
            if (!string.Equals(news.Alias, alias, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToActionPermanent("Details", new { id = news.Id, alias = news.Alias });
            }

            return View(news);
        }
    }
}
