// Areas/Admin/Controllers/DiscountController.cs
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
    public class DiscountsController : Controller
    {
        private readonly NbshopContext _db;

        public DiscountsController(NbshopContext db)
        {
            _db = db;
        }

        // GET: Admin/Discount
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            const int pageSize = 20;
            var query = _db.CatalogDiscounts
                .Include(d => d.Product)
                .AsQueryable();

            // Поиск по названию товара или ID скидки
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d =>
                    d.Product!.Name.Contains(search) ||
                    d.Id.ToString() == search);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.Id) // свежие сверху
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DiscountListItemVM
                {
                    Id = d.Id,
                    ProductName = d.Product!.Name,
                    DiscountType = d.DiscountType,
                    Value = d.Value,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    IsActive = d.IsActive,
                    Priority = d.Priority
                })
                .ToListAsync();

            var model = new DiscountListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchQuery = search
            };

            return View(model);
        }

        // GET: Admin/Discount/Create
        public async Task<IActionResult> Create()
        {
            await LoadProductsViewBag();
            return View(new DiscountEditVM
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1) 
            });
        }

        // POST: Admin/Discount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountEditVM model)
        {
            // Дополнительная проверка дат
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("", "Дата окончания должна быть позже даты начала");
            }

            if (ModelState.IsValid)
            {
                var discount = new CatalogDiscount
                {
                    ProductId = model.ProductId,
                    DiscountType = (int)model.DiscountType,
                    Value = model.Value,
                    StartDate = model.StartDate.ToUniversalTime(),
                    EndDate = model.EndDate.ToUniversalTime(),
                    IsActive = model.IsActive,
                    Priority = model.Priority
                };

                _db.Add(discount);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Скидка успешно создана";
                return RedirectToAction(nameof(Index));
            }

            await LoadProductsViewBag();
            return View(model);
        }

        // GET: Admin/Discount/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var discount = await _db.CatalogDiscounts.FindAsync(id);
            if (discount == null)
                return NotFound();

            var model = new DiscountEditVM
            {
                Id = discount.Id,
                ProductId = discount.ProductId,
                DiscountType = (DiscountType)discount.DiscountType,
                Value = discount.Value,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive,
                Priority = discount.Priority
            };

            await LoadProductsViewBag();
            return View(model);
        }

        // POST: Admin/Discount/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DiscountEditVM model)
        {
            if (id != model.Id)
                return NotFound();

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("", "Дата окончания должна быть позже даты начала");
            }

            if (ModelState.IsValid)
            {
                var discount = await _db.CatalogDiscounts.FindAsync(id);
                if (discount == null)
                    return NotFound();

                discount.ProductId = model.ProductId;
                discount.DiscountType = (int)model.DiscountType;
                discount.Value = model.Value;
                discount.StartDate = model.StartDate.ToUniversalTime();
                discount.EndDate = model.EndDate.ToUniversalTime();
                discount.IsActive = model.IsActive;
                discount.Priority = model.Priority;

                await _db.SaveChangesAsync();
                TempData["Success"] = "Скидка обновлена";
                return RedirectToAction(nameof(Index));
            }

            await LoadProductsViewBag();
            return View(model);
        }

        // POST: Admin/Discount/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var discount = await _db.CatalogDiscounts.FindAsync(id);
            if (discount == null)
                return NotFound();

            _db.CatalogDiscounts.Remove(discount);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Скидка удалена";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для загрузки списка товаров в ViewBag
        private async Task LoadProductsViewBag()
        {
            var products = await _db.CatalogProducts
                .Where(p => p.Active) // можно убрать фильтр, если нужно показывать все
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name");
        }
    }
}