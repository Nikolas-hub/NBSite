using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBSite.Areas.Admin.Models;

namespace NBSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class UsersController : Controller
    {
        private readonly NbshopContext _db;

        public UsersController(NbshopContext db)
        {
            _db = db;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;

            var query = _db.AuthUsers
                .Include(u => u.AccountsProfile)
                .Where(u => u.IsActive) // по умолчанию показываем только активных? Можно убрать, чтобы видеть всех.
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (long.TryParse(search, out long userId))
                {
                    query = query.Where(u => u.Id == userId);
                }
                else
                {
                    query = query.Where(u =>
                        u.Email.Contains(search) ||
                        u.Username.Contains(search) ||
                        (u.FirstName != null && u.FirstName.Contains(search)) ||
                        (u.LastName != null && u.LastName.Contains(search)) ||
                        (u.AccountsProfile != null && u.AccountsProfile.Phone!.Contains(search)));
                }
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = users.Select(u => new UserListItemVM
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.AccountsProfile?.Phone,
                IsActive = u.IsActive,
                IsStaff = u.IsStaff,
                IsSuperuser = u.IsSuperuser,
                DateJoined = u.DateJoined,
                LastLogin = u.LastLogin,
                OrdersCount = _db.CatalogOrders.Count(o => o.UserId == u.Id && o.Status != (long)OrderStatus.Cart)
            }).ToList();

            var model = new UserListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        public async Task<IActionResult> Details(long id)
        {
            var user = await _db.AuthUsers
                .Include(u => u.AccountsProfile)
                    .ThenInclude(p => p!.City)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Получаем заказы без преобразования статуса в строку
            var orders = await _db.CatalogOrders
                .Include(o => o.CatalogOrderproducts)
                .Where(o => o.UserId == id && o.Status != (long)OrderStatus.Cart)
                .OrderByDescending(o => o.Id)
                .Select(o => new UserOrderVM
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    StatusValue = (int)o.Status,
                    Total = o.CatalogOrderproducts.Sum(p => p.Price * p.Quantity),
                    ItemsCount = o.CatalogOrderproducts.Count
                })
                .ToListAsync();

            // Теперь в памяти заполняем название статуса
            foreach (var order in orders)
            {
                order.Status = GetStatusName(order.StatusValue);
            }

            var model = new UserDetailsVM
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.AccountsProfile?.Phone,
                Company = user.AccountsProfile?.Company,
                CompanyPost = user.AccountsProfile?.CompanyPost,
                CityName = user.AccountsProfile?.City?.Name,
                IsActive = user.IsActive,
                IsStaff = user.IsStaff,
                IsSuperuser = user.IsSuperuser,
                DateJoined = user.DateJoined,
                LastLogin = user.LastLogin,
                Orders = orders
            };

            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var user = await _db.AuthUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserEditVM
            {
                Id = user.Id,
                IsActive = user.IsActive,
                IsStaff = user.IsStaff,
                IsSuperuser = user.IsSuperuser
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.AuthUsers.FindAsync(model.Id);
            if (user == null)
                return NotFound();

            user.IsActive = model.IsActive;
            user.IsStaff = model.IsStaff;
            user.IsSuperuser = model.IsSuperuser;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Данные пользователя обновлены";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await _db.AuthUsers
                .Include(u => u.AccountsProfile)
                .Include(u => u.CatalogOrders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Проверка на наличие заказов
            if (user.CatalogOrders.Any(o => o.Status != (long)OrderStatus.Cart))
            {
                // Мягкое удаление - деактивируем
                user.IsActive = false;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Пользователь деактивирован (у него есть заказы)";
                return RedirectToAction(nameof(Index));
            }

            // Удаляем профиль и пользователя
            if (user.AccountsProfile != null)
                _db.AccountsProfiles.Remove(user.AccountsProfile);

            _db.AuthUsers.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Пользователь удалён";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для получения названия статуса заказа
        private string GetStatusName(int status)
        {
            return status switch
            {
                (int)OrderStatus.Submitted => "Оформлен",
                (int)OrderStatus.Exported => "Экспортирован",
                _ => "Неизвестно"
            };
        }
    }
}