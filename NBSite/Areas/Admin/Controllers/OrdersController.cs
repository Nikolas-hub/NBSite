using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBSite.Areas.Admin.Models;

namespace NBSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class OrdersController : Controller
    {
        private readonly NbshopContext _db;

        // Словарь для отображения числовых статусов в понятные названия
        private static readonly Dictionary<int, string> _statusNames = new()
        {
            { 0, "Корзина" },
            { 1, "Оформлен" },
            { 2, "Экспортирован" }
            // Добавьте сюда другие статусы, если они есть в БД, например:
            // { 3, "Оплачен" },
            // { 4, "Доставлен" },
            // { 5, "Отменён" }
        };

        public OrdersController(NbshopContext db)
        {
            _db = db;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(int page = 1, string search = "", int? status = null)
        {
            const int pageSize = 20;

            var query = _db.CatalogOrders
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.City)
                .Include(o => o.User).ThenInclude(u => u!.AccountsProfile)
                .Include(o => o.CatalogOrderproducts)
                .Where(o => o.Status != (long)OrderStatus.Cart); // Исключаем корзины

            if (!string.IsNullOrEmpty(search))
            {
                if (long.TryParse(search, out long orderId))
                {
                    query = query.Where(o => o.Id == orderId);
                }
                else
                {
                    query = query.Where(o =>
                        o.Email!.Contains(search) ||
                        o.Reciever!.Contains(search) ||
                        o.Phone!.Contains(search));
                }
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = orders.Select(o => new OrderListItemVM
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                SubmittedAt = o.SubmittedAt,
                StatusValue = (int)o.Status,
                Status = GetStatusName((int)o.Status),
                Reciever = o.Reciever,
                Email = o.Email,
                Phone = o.Phone,
                Total = o.CatalogOrderproducts.Sum(p => p.Price * p.Quantity),
                ItemsCount = o.CatalogOrderproducts.Count,
                DeliveryName = o.Delivery?.Name ?? "Не указано",
                PaymentName = o.Payment?.Name ?? "Не указано",
                CityName = o.City?.Name ?? "Не указано",
                UserName = o.User?.Username ?? (o.User?.AccountsProfile?.Fio ?? "Неизвестно")
            }).ToList();

            var model = new OrderListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search,
                StatusFilter = status
            };

            ViewBag.Statuses = GetStatusSelectList(status);

            return View(model);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var order = await _db.CatalogOrders
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.City)
                .Include(o => o.User).ThenInclude(u => u!.AccountsProfile)
                .Include(o => o.CatalogOrderproducts).ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.Status != (long)OrderStatus.Cart);

            if (order == null)
                return NotFound();

            var products = order.CatalogOrderproducts.Select(op => new OrderProductItemVM
            {
                ProductId = op.ProductId,
                ProductName = op.Product?.Name ?? "Товар удален",
                ProductAlias = op.Product?.Alias,
                Price = op.Price,
                Quantity = op.Quantity
            }).ToList();

            var subtotal = products.Sum(p => p.Total);

            var model = new OrderDetailsVM
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                SubmittedAt = order.SubmittedAt,
                StatusValue = (int)order.Status,
                Status = GetStatusName((int)order.Status),
                Reciever = order.Reciever,
                Email = order.Email,
                Phone = order.Phone,
                Comment = order.Comment,
                DeliveryName = order.Delivery?.Name ?? "Не указано",
                DeliveryId = order.DeliveryId,
                PaymentName = order.Payment?.Name ?? "Не указано",
                PaymentId = order.PaymentId,
                CityName = order.City?.Name ?? "Не указано",
                CityId = order.CityId,
                // CouponCode = order.Coupon?.Code, // если есть связь с купоном
                UserId = order.UserId,
                UserName = order.User?.Username ?? (order.User?.AccountsProfile?.Fio ?? "Неизвестно"),
                Products = products,
                Subtotal = subtotal,
                Total = subtotal // прибавить стоимость доставки, если есть
            };

            ViewBag.Statuses = GetStatusSelectList((int)order.Status);

            return View(model);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(OrderStatusUpdateVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var order = await _db.CatalogOrders.FindAsync(model.OrderId);
            if (order == null)
                return NotFound();

            order.Status = model.NewStatus;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Статус заказа обновлён";
            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        // Вспомогательные методы
        private string GetStatusName(int status)
        {
            return _statusNames.TryGetValue(status, out var name) ? name : $"Неизвестно ({status})";
        }

        private string GetStatusBadgeClass(int status)
        {
            return status switch
            {
                1 => "bg-primary",      // Оформлен
                2 => "bg-success",      // Экспортирован
                0 => "bg-secondary",    // Корзина (обычно не показывается)
                _ => "bg-secondary"
            };
        }

        private List<SelectListItem> GetStatusSelectList(int? selected = null)
        {
            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Все статусы" }
            };

            foreach (var kv in _statusNames.Where(kv => kv.Key != 0)) // исключаем корзину
            {
                statuses.Add(new SelectListItem
                {
                    Value = kv.Key.ToString(),
                    Text = kv.Value,
                    Selected = selected == kv.Key
                });
            }
            return statuses;
        }
    }
}