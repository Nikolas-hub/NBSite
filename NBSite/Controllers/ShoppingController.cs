using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBSite.Infrastructure;
using NBSite.Models;
using NBSite.Models.ViewComponents;
using System.Security.Claims;

namespace NBSite.Controllers
{
    public class ShoppingController : Controller
    {
        private readonly NbshopContext _db;

        private readonly IEmailSender _emailSender;
        private readonly AppConfig _appConfig;
        private readonly ILogger<ShoppingController> _logger;

        public ShoppingController(NbshopContext db, IEmailSender emailSender, AppConfig appConfig, ILogger<ShoppingController> logger)
        {
            _db = db;
            _emailSender = emailSender;
            _appConfig = appConfig;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ShowProducts(
        string? categoryAlias,
        string sortOrder = "name_asc",
        int page = 1,
        string? query = null,
        bool? inStock = null,
        double? minPrice = null,
        double? maxPrice = null,
        string? categoryIds = null)   // строка вида "1,2,5"
        {
            if (!inStock.HasValue)
            {
                inStock = true;
            }

            await LoadMenuCategories();
            await GetCartInfo();

            // Получаем выбранные категории
            var selectedCatIds = new List<long>();
            if (!string.IsNullOrEmpty(categoryIds))
            {
                selectedCatIds = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                             .Select(long.Parse)
                                             .ToList();
            }

            // Загружаем все категории для дерева фильтра
            var allCategories = await GetCategoryTreeForFilter();
            MarkSelectedCategories(allCategories, selectedCatIds);

            // Определяем категорию по alias (если передан)
            CatalogCategory? category = null;
            if (!string.IsNullOrEmpty(categoryAlias))
            {
                category = await _db.CatalogCategories
                    .FirstOrDefaultAsync(c => c.Alias == categoryAlias && c.Active == true);
                if (category == null) return NotFound();
            }


            // Базовый запрос товаров
            IQueryable<CatalogProduct> productsQuery = _db.CatalogProducts
                .Where(p => p.Active == true)
                .Include(p => p.Category)
                .Include(p => p.Manufacturer);

            // Фильтр по наличию
            if (inStock == true)
                productsQuery = productsQuery.Where(p => p.Quantity > 0);

            // Фильтр по категориям
            if (selectedCatIds.Any())
            {
                // Получаем все Id категорий с учётом подкатегорий
                var allCategoryIds = await GetAllCategoryIdsIncludingChildren(selectedCatIds);
                productsQuery = productsQuery.Where(p => p.CategoryId.HasValue && allCategoryIds.Contains(p.CategoryId.Value));
            }
            else if (category != null)
            {
                // Если выбрана конкретная категория (через alias)
                if (!string.IsNullOrEmpty(category.Parent))
                {
                    // Это подкатегория – только её товары
                    productsQuery = productsQuery.Where(p => p.CategoryId == category.Id);
                }
                else
                {
                    // Корневая категория – включаем все подкатегории
                    var childCategoryCodes = await _db.CatalogCategories
                        .Where(c => c.Parent == category.Code && c.Active == true)
                        .Select(c => c.Code)
                        .ToListAsync();

                    var childCategoryIds = await _db.CatalogCategories
                        .Where(c => childCategoryCodes.Contains(c.Code))
                        .Select(c => c.Id)
                        .ToListAsync();

                    // Добавляем саму категорию
                    childCategoryIds.Add(category.Id);

                    productsQuery = productsQuery.Where(p => p.CategoryId.HasValue && childCategoryIds.Contains(p.CategoryId.Value));
                }
            }

            // Фильтр по поисковому запросу
            if (!string.IsNullOrEmpty(query))
            {
                var normalizedQuery = query.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(normalizedQuery) ||
                    (p.Introtext != null && p.Introtext.ToLower().Contains(normalizedQuery)) ||
                    (p.SearchKeywords != null && p.SearchKeywords.ToLower().Contains(normalizedQuery)) ||
                    (p.Manufacturer != null && p.Manufacturer.Name.ToLower().Contains(normalizedQuery)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(normalizedQuery)) ||
                    (p.Id.ToString().Contains(normalizedQuery)));
            }

            // Фильтр по цене (произвольный диапазон)
            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

            // Сортировка
            productsQuery = sortOrder switch
            {
                "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "new" => productsQuery
                    .Where(p => p.New == true)
                    .OrderByDescending(p => p.Id), // самые новые сверху (по ID)
                "popular" => productsQuery
                    .Where(p => p.Popular == true)
                    .OrderByDescending(p => p.Popular).ThenByDescending(p => p.Id),

                "discount" => productsQuery
                    .Where(p => p.CatalogDiscounts!.Any(d => d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow))
                     .Select(p => new
                     {
                         Product = p,
                         MaxDiscount = p.CatalogDiscounts!
                            .Where(d => d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                            .Max(d => (decimal?)d.Value) ?? 0  // используем nullable для случая, если скидок нет
                     })
                    .OrderByDescending(x => x.MaxDiscount)
                    .ThenByDescending(x => x.Product.Id)
                    .Select(x => x.Product),
                _ => productsQuery.OrderBy(p => p.Name),
            };


            var totalCount = await productsQuery.CountAsync();
            int pageSize = 12;
            // Получаем товары для текущей страницы (без скидок)
            var products = await productsQuery
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
                    IsNew = p.New,
                    IsPopular = p.Popular,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    CategoryAlias = p.Category.Alias,
                    ManufacturerId = p.ManufacturerId,
                    ManufacturerName = p.Manufacturer!.Name,
                    Quantity = p.Quantity,
                    Multiplicity = p.Multiplicity
                })
                .ToListAsync();

            // Загружаем активные скидки для этих товаров
            var productIds = products.Select(p => p.Id).ToList();
            var discounts = await _db.CatalogDiscounts
                .Where(d => productIds.Contains(d.ProductId) && d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            // Применяем скидки к товарам
            foreach (var product in products)
            {
                var discount = discounts.FirstOrDefault(d => d.ProductId == product.Id);
                if (discount != null)
                {
                    product.HasDiscount = true;
                    // Рассчитываем цену со скидкой в зависимости от типа
                    if (discount.DiscountType == 3) // Новая цена
                    {
                        product.DiscountedPrice = discount.Value;
                    }
                    else if (discount.DiscountType == 2) // Фиксированная сумма
                    {
                        product.DiscountedPrice = (decimal)product.Price - discount.Value;
                        if (product.Price > 0)
                            product.DiscountPercent = (int)(discount.Value / (decimal)product.Price * 100);
                    }
                    else if (discount.DiscountType == 1) // Процент
                    {
                        product.DiscountedPrice = (decimal)product.Price * (1 - discount.Value / 100m);
                        product.DiscountPercent = (int)discount.Value;
                    }
                }
            }
            

            var model = new ShowProductsVM
            {
                Category = category,
                Products = products,
                SortOrder = sortOrder,
                Query = query,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                InStock = inStock,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedCategoryIds = selectedCatIds,
                AllCategories = allCategories
            };
            if (category != null)
            {
                model.CategoryPath = await GetCategoryPath(category);
            }

            return View(model);
        }

        private async Task<List<CatalogCategory>> GetCategoryPath(CatalogCategory category)
        {
            var path = new List<CatalogCategory> { category };
            var current = category;
            while (!string.IsNullOrEmpty(current.Parent))
            {
                var parent = await _db.CatalogCategories
                    .FirstOrDefaultAsync(c => c.Code == current.Parent && c.Active == true);
                if (parent == null) break;
                path.Insert(0, parent);
                current = parent;
            }
            return path;
        }

        // Получение всех подкатегорий для выбранных Id
        private async Task<List<long>> GetAllCategoryIdsIncludingChildren(List<long> selectedIds)
        {
            var allCategories = await _db.CatalogCategories
                .Where(c => c.Active)
                .Select(c => new { c.Id, c.Code, c.Parent })
                .ToListAsync();

            var childrenByParentCode = allCategories
                .Where(c => !string.IsNullOrEmpty(c.Parent))
                .GroupBy(c => c.Parent!)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var result = new HashSet<long>(selectedIds);
            var queue = new Queue<long>(selectedIds);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var currentCode = allCategories.FirstOrDefault(c => c.Id == currentId)?.Code;
                if (currentCode == null) continue;

                if (childrenByParentCode.TryGetValue(currentCode, out var childIds))
                {
                    foreach (var childId in childIds)
                    {
                        if (result.Add(childId))
                            queue.Enqueue(childId);
                    }
                }
            }

            return result.ToList();
        }

        // Получение дерева категорий для фильтра
        private async Task<List<CategoryFilterDto>> GetCategoryTreeForFilter()
        {
            var categories = await _db.CatalogCategories
                .Where(c => c.Active)
                .Select(c => new CategoryFilterDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Alias = c.Alias,
                    Code = c.Code!,
                    ParentCode = c.Parent
                })
                .ToListAsync();

            // Строим дерево
            var lookup = categories.ToLookup(c => c.ParentCode);
            foreach (var cat in categories)
            {
                cat.Children = lookup[cat.Code].ToList();
            }

            // Возвращаем корневые
            return categories.Where(c => c.ParentCode == null).ToList();
        }

        // Отметить выбранные категории в дереве
        private void MarkSelectedCategories(List<CategoryFilterDto> categories, List<long> selectedIds)
        {
            foreach (var cat in categories)
            {
                cat.Selected = selectedIds.Contains(cat.Id);
                if (cat.Children.Any())
                    MarkSelectedCategories(cat.Children, selectedIds);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowProduct([FromRoute(Name = "id")] string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return NotFound();
            }

            // Загружаем категории для меню
            await LoadMenuCategories();

            // Получаем корзину для текущего пользователя
            await GetCartInfo();

            // Ищем товар по alias
            var product = await _db.CatalogProducts
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.Alias == alias && p.Active == true);

            if (product == null)
            {
                return NotFound();
            }

            // Находим связанные товары (из той же категории)
            var relatedProducts = await _db.CatalogProducts
                .Where(p => p.Active == true &&
                           p.CategoryId == product.CategoryId &&
                           p.Id != product.Id)
                .OrderByDescending(p => p.Popular)
                .ThenByDescending(p => p.Id)
                .Take(4)
                .Select(p => new ProductPreviewVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Alias = p.Alias,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    Image = p.Image,
                    CategoryAlias = p.Category!.Alias
                })
                .ToListAsync();

            // Проверяем, подписан ли текущий пользователь на уведомление о поступлении
            bool isSubscribed = false;
            if (User.Identity!.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(userEmail))
                {
                    isSubscribed = await _db.CatalogInstocksubscriptions
                        .AnyAsync(s => s.ProductId == product.Id &&
                                      s.Email == userEmail &&
                                      s.Status == (long)SubscriptionStatus.Added);
                }
            }

            var model = new ShowProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Alias = product.Alias,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Image = product.Image,
                Content = product.Content,
                IntroText = product.Introtext,
                Quantity = product.Quantity,
                Multiplicity = product.Multiplicity,
                IsNew = product.New,
                IsPopular = product.Popular,
                HasReject = product.HasReject,
                Volume = product.Volume,
                Weight = product.Weight,
                Ean13 = product.Ean13,
                Manual = product.Manual,
                ExpirationDate = product.ExpirationDate,
                CategoryId = product.CategoryId,
                Category = product.Category,
                CategoryAlias = product.Category?.Alias,
                ManufacturerId = product.ManufacturerId,
                Manufacturer = product.Manufacturer,
                RelatedProducts = relatedProducts,
                IsSubscribedToStock = isSubscribed,

            };
            if (product.Category != null)
            {
                model.CategoryPath = await GetCategoryPath(product.Category);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToBasket(AddToBasketVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Неверные данные");

            try
            {
                // Получаем корзину через универсальный метод (работает для всех)
                var cart = await GetOrCreateCartAsync();

                var product = await _db.CatalogProducts.FindAsync(model.ProductId);
                if (product == null)
                    return NotFound("Товар не найден");

                // Проверка кратности упаковки
                if (model.Quantity % product.Multiplicity != 0)
                    model.Quantity = (model.Quantity / product.Multiplicity + 1) * product.Multiplicity;

                // Ищем товар в корзине
                var cartItem = cart.CatalogOrderproducts.FirstOrDefault(p => p.ProductId == model.ProductId);

                if (cartItem != null)
                {
                    // Обновляем количество
                    cartItem.Quantity += model.Quantity;

                    // Проверяем остаток на складе
                    if (product.Quantity > 0 && cartItem.Quantity > product.Quantity)
                        cartItem.Quantity = product.Quantity;
                }
                else
                {
                    // Добавляем новый товар
                    cartItem = new CatalogOrderproduct
                    {
                        OrderId = cart.Id,
                        ProductId = model.ProductId,
                        Price = product.Price,
                        Quantity = model.Quantity,
                        Weight = product.Weight,
                        Volume = product.Volume
                    };
                    _db.CatalogOrderproducts.Add(cartItem);
                }

                await _db.SaveChangesAsync();

                var itemCount = cart.CatalogOrderproducts.Sum(p => p.Quantity);
                var totalPrice = cart.CatalogOrderproducts.Sum(p => p.Price * p.Quantity);

                return Json(new
                {
                    success = true,
                    message = "Товар добавлен в корзину",
                    cartItemCount = itemCount,
                    cartTotal = totalPrice
                });
            }
            catch (Exception ex)
            {
                // Логируйте исключение здесь (например, ILogger)
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowBasket()
        {
            await LoadMenuCategories();

            // Получаем корзину
            var cart = await GetOrCreateCartAsync();

            // Загружаем справочники
            var deliveries = await _db.CatalogDeliveries
                .OrderBy(d => d.Sort)
                .ThenBy(d => d.Name)
                .ToListAsync();

            var payments = await _db.CatalogPayments
                .OrderBy(p => p.Name)
                .ToListAsync();

            var cities = await _db.ReferencesCities
                .Where(c => c.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var model = new ShowBasketVM
            {
                Cart = cart,
                Deliveries = deliveries,
                Payments = payments,
                Cities = cities
            };

            // Если пользователь авторизован – заполняем контактные данные
            if (User.Identity?.IsAuthenticated == true)
            {
                var profile = await GetUserProfileAsync();
                if (profile != null)
                {
                    model.Name = profile.Fio ?? string.Empty;
                    model.Email = profile.Email ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                    model.Phone = profile.Phone ?? string.Empty;
                }
                else
                {
                    // Если профиль не найден, используем claims (например, email)
                    model.Name = string.Empty;
                    model.Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                    model.Phone = string.Empty;
                }
            }
            else
            {
                // Для неавторизованных оставляем поля пустыми (форма не будет показана)
                model.Name = string.Empty;
                model.Email = string.Empty;
                model.Phone = string.Empty;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int itemId, int quantity)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(); // всегда существует

                var cartItem = cart.CatalogOrderproducts.FirstOrDefault(p => p.Id == itemId);
                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Товар не найден в корзине" });
                }

                var product = await _db.CatalogProducts.FindAsync(cartItem.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Товар не найден" });
                }

                if (quantity <= 0)
                {
                    _db.CatalogOrderproducts.Remove(cartItem);
                }
                else
                {
                    // Проверяем кратность
                    if (quantity % product.Multiplicity != 0)
                    {
                        quantity = (quantity / product.Multiplicity + 1) * product.Multiplicity;
                    }

                    // Проверяем остаток
                    if (product.Quantity > 0 && quantity > product.Quantity)
                    {
                        quantity = product.Quantity;
                    }

                    cartItem.Quantity = quantity;
                }

                await _db.SaveChangesAsync();

                // Пересчитываем итоги
                var totalItems = cart.CatalogOrderproducts.Sum(p => p.Quantity);
                var totalPrice = cart.CatalogOrderproducts.Sum(p => p.Price * p.Quantity);
                var itemPrice = cartItem.Quantity * cartItem.Price;

                return Json(new
                {
                    success = true,
                    totalItems,
                    totalPrice,
                    itemPrice,
                    itemQuantity = cartItem.Quantity
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ошибка при обновлении корзины" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(); // всегда существует

                var cartItem = cart.CatalogOrderproducts.FirstOrDefault(p => p.Id == itemId);
                if (cartItem != null)
                {
                    _db.CatalogOrderproducts.Remove(cartItem);
                    await _db.SaveChangesAsync();
                }

                var totalItems = cart.CatalogOrderproducts.Sum(p => p.Quantity);
                var totalPrice = cart.CatalogOrderproducts.Sum(p => p.Price * p.Quantity);

                return Json(new
                {
                    success = true,
                    totalItems,
                    totalPrice
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ошибка при удалении товара" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeliveryAndPayment(UpdateCartVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Неверные данные");

            try
            {
                var cart = await GetOrCreateCartAsync();
                cart.DeliveryId = model.DeliveryId;
                cart.PaymentId = model.PaymentId;
                cart.CityId = model.CityId;
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Данные обновлены" });
            }
            catch
            {
                return Json(new { success = false, message = "Ошибка при обновлении данных" });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder(SubmitOrderVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("ShowBasket", model);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var cart = await _db.CatalogOrders
                    .Include(o => o.CatalogOrderproducts)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == (long)OrderStatus.Cart);

                if (cart == null || !cart.CatalogOrderproducts.Any())
                {
                    return RedirectToAction("ShowBasket");
                }

                // Обновляем данные заказа
                cart.Status = (long)OrderStatus.Submitted;
                cart.SubmittedAt = DateTime.UtcNow;
                cart.Reciever = model.Name;
                cart.Email = model.Email;
                cart.Phone = model.Phone;
                cart.Comment = model.Comment;
                cart.DeliveryId = model.DeliveryId;
                cart.PaymentId = model.PaymentId;
                cart.CityId = model.CityId;

                //// Если использован купон, деактивируем его
                //if (!string.IsNullOrEmpty(model.CouponCode))
                //{
                //    var coupon = await _db.CatalogCoupons
                //        .Include(c => c.Promo)
                //        .FirstOrDefaultAsync(c => c.Code == model.CouponCode && c.IsActive);

                //    if (coupon != null)
                //    {
                //        cart.CouponId = coupon.Id;
                //        coupon.IsActive = false;
                //    }
                //}

                // Проверяем остатки и обновляем их
                foreach (var item in cart.CatalogOrderproducts)
                {
                    var product = await _db.CatalogProducts.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (product.Quantity > 0)
                        {
                            product.Quantity = (int)Math.Max(0, product.Quantity - item.Quantity);
                        }
                    }
                }

                await _db.SaveChangesAsync();

                var companyEmail = _appConfig.Company.CompanyEmail;
                var emailBody = BuildOrderEmailBody(cart);
                try
                {
                    await _emailSender.SendEmailAsync(companyEmail!, $"Новый заказ №{cart.Id}", emailBody, true);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Ошибка отправки письма");
                }

                return RedirectToAction("OrderConfirmation", new { orderId = cart.Id });
            }
            //TODO: Можно сделать более подробное логгирование ошибок
            catch (Exception)
            {
                ModelState.AddModelError("", "Произошла ошибка при оформлении заказа");
                return await ShowBasket();
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _db.CatalogOrders
                .Include(o => o.CatalogOrderproducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.City)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Загружаем категории для меню
            await LoadMenuCategories();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeToStock(long productId, string email)
        {
            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                return Json(new { success = false, message = "Неверный email адрес" });

            try
            {
                // Проверяем, не подписан ли уже этот email (статус 0 – активен)
                var existing = await _db.CatalogInstocksubscriptions
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.Email == email && s.Status == 0);
                if (existing != null)
                    return Json(new { success = false, message = "Вы уже подписаны на уведомление" });

                var subscription = new CatalogInstocksubscription
                {
                    ProductId = productId,
                    Email = email,
                    Status = 0
                };

                _db.CatalogInstocksubscriptions.Add(subscription);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Вы успешно подписались на уведомление" });
            }
            catch
            {
                return Json(new { success = false, message = "Ошибка при подписке" });
            }
        }



        private async Task<CatalogOrder> GetOrCreateCartAsync()
        {
            const string sessionKey = "AnonymousCartId";

            // Авторизованный пользователь
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userCart = await _db.CatalogOrders
                    .Include(o => o.CatalogOrderproducts)
                        .ThenInclude(op => op.Product)
                            .ThenInclude(p => p!.Category)
                    .Include(o => o.CatalogOrderproducts)
                        .ThenInclude(op => op.Product)
                            .ThenInclude(p => p!.Manufacturer)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == (long)OrderStatus.Cart);

                // Проверяем наличие анонимной корзины в сессии
                long? anonymousCartId = HttpContext.Session.GetInt32(sessionKey);
                if (anonymousCartId.HasValue)
                {
                    var anonymousCart = await _db.CatalogOrders
                        .Include(o => o.CatalogOrderproducts)
                            .ThenInclude(op => op.Product)
                                .ThenInclude(p => p!.Category)
                        .Include(o => o.CatalogOrderproducts)
                            .ThenInclude(op => op.Product)
                                .ThenInclude(p => p!.Manufacturer)
                        .FirstOrDefaultAsync(o => o.Id == anonymousCartId.Value && o.Status == (long)OrderStatus.Cart);

                    if (anonymousCart != null)
                    {
                        if (userCart != null)
                        {
                            // Объединяем товары
                            foreach (var item in anonymousCart.CatalogOrderproducts.ToList()) // ToList() для безопасной итерации
                            {
                                var existingItem = userCart.CatalogOrderproducts
                                    .FirstOrDefault(p => p.ProductId == item.ProductId);
                                if (existingItem != null)
                                {
                                    // Товар уже есть в корзине пользователя — увеличиваем количество
                                    existingItem.Quantity += item.Quantity;
                                    // Удаляем старую запись товара (она больше не нужна)
                                    _db.CatalogOrderproducts.Remove(item);
                                }
                                else
                                {
                                    // Переносим товар в корзину пользователя
                                    item.OrderId = userCart.Id;
                                    // Добавляем товар в коллекцию корзины пользователя
                                    userCart.CatalogOrderproducts.Add(item);
                                    // Удаляем товар из коллекции анонимной корзины (разрываем связь в памяти)
                                    anonymousCart.CatalogOrderproducts.Remove(item);
                                }
                            }
                            // После цикла коллекция anonymousCart.CatalogOrderproducts пуста
                            // Теперь можно безопасно удалить анонимную корзину
                            _db.CatalogOrders.Remove(anonymousCart);
                        }
                        else
                        {
                            // У пользователя ещё нет корзины — просто привязываем анонимную
                            anonymousCart.UserId = userId;
                            userCart = anonymousCart;
                        }

                        await _db.SaveChangesAsync();
                        HttpContext.Session.Remove(sessionKey);
                    }
                }

                if (userCart != null)
                    return userCart;

                // Создаём новую корзину для авторизованного пользователя
                userCart = new CatalogOrder
                {
                    UserId = userId,
                    Status = (long)OrderStatus.Cart,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CatalogOrders.Add(userCart);
                await _db.SaveChangesAsync();
                return userCart;
            }
            else // Анонимный пользователь
            {
                // Пытаемся получить существующую анонимную корзину
                long? cartId = HttpContext.Session.GetInt32(sessionKey);
                if (cartId.HasValue)
                {
                    var cart = await _db.CatalogOrders
                        .Include(o => o.CatalogOrderproducts)
                            .ThenInclude(op => op.Product)
                                .ThenInclude(p => p!.Category)
                        .Include(o => o.CatalogOrderproducts)
                            .ThenInclude(op => op.Product)
                                .ThenInclude(p => p!.Manufacturer)
                        .FirstOrDefaultAsync(o => o.Id == cartId.Value && o.Status == (long)OrderStatus.Cart);
                    if (cart != null)
                        return cart;
                }

                // Создаём новую анонимную корзину
                var newCart = new CatalogOrder
                {
                    UserId = null,
                    Status = (long)OrderStatus.Cart,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CatalogOrders.Add(newCart);
                await _db.SaveChangesAsync();

                HttpContext.Session.SetInt32(sessionKey, (int)newCart.Id);
                return newCart;
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderHistory(int page = 1)
        {
            // Загружаем категории для меню
            await LoadMenuCategories();

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ordersQuery = _db.CatalogOrders
                .Where(o => o.UserId == userId && o.Status != (long)OrderStatus.Cart)
                .Include(o => o.CatalogOrderproducts) // обязательно для подсчёта суммы
                .OrderByDescending(o => o.Id);

            var totalCount = await ordersQuery.CountAsync();
            int pageSize = 10;

            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Преобразуем в ViewModel
            var items = orders.Select(o => new OrderHistoryItemVM
            {
                Id = (int)o.Id,
                CreatedAt = o.CreatedAt,
                Status = GetStatusName((int)o.Status), // используем вспомогательный метод
                Total = o.CatalogOrderproducts.Sum(p => p.Price * p.Quantity),
                ItemsCount = o.CatalogOrderproducts.Count
            }).ToList();

            var model = new OrderHistoryVM
            {
                Orders = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(model);
        }

        private string GetStatusName(int status)
        {
            return status switch
            {
                (int)OrderStatus.Submitted => "Оформлен",
                (int)OrderStatus.Exported => "Экспортирован",
                _ => "Неизвестно"
            };
        }

        private async Task LoadMenuCategories()
        {
            var categories = await _db.CatalogCategories
                .Where(c => c.Active == true)
                //.Include(c => c.Children.Where(ch => ch.Active == true))
                .Where(c => string.IsNullOrEmpty(c.Parent))
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewData["MenuCategories"] = categories;
        }

        private async Task GetCartInfo()
        {
            var cart = await GetCartAsync();
            if (cart != null)
            {
                ViewData["CartItemCount"] = cart.CatalogOrderproducts.Sum(p => p.Quantity);
                ViewData["BasketTotal"] = cart.CatalogOrderproducts.Sum(p => p.Price * p.Quantity);
            }
            else
            {
                ViewData["CartItemCount"] = 0;
                ViewData["BasketTotal"] = 0;
            }
        }

        private async Task<CatalogOrder?> GetCartAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                return await _db.CatalogOrders
                    .Include(o => o.CatalogOrderproducts)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == (long)OrderStatus.Cart);
            }
            else
            {
                const string sessionKey = "AnonymousCartId";
                long? cartId = HttpContext.Session.GetInt32(sessionKey);
                if (cartId.HasValue)
                {
                    return await _db.CatalogOrders
                        .Include(o => o.CatalogOrderproducts)
                        .FirstOrDefaultAsync(o => o.Id == cartId.Value && o.Status == (long)OrderStatus.Cart);
                }
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var cart = await GetOrCreateCartAsync();
            var count = cart?.CatalogOrderproducts.Sum(p => p.Quantity) ?? 0;
            var total = cart?.CatalogOrderproducts.Sum(p => p.Price * p.Quantity) ?? 0;
            return Json(new { success = true, count, total });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                _db.CatalogOrderproducts.RemoveRange(cart.CatalogOrderproducts);
                await _db.SaveChangesAsync();
                return Json(new { success = true, redirectUrl = Url.Action("ShowBasket") });
            }
            catch
            {
                return Json(new { success = false, message = "Ошибка при очистке корзины" });
            }
        }

        private async Task<AccountsProfile?> GetUserProfileAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return null;
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _db.AccountsProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        private string BuildOrderEmailBody(CatalogOrder order)
        {
            var itemsHtml = string.Join("", order.CatalogOrderproducts.Select(item => $@"
        <tr>
            <td>{item.Product?.Name}</td>
            <td>{item.Price:N2} ₽</td>
            <td>{item.Quantity}</td>
            <td>{(item.Price * item.Quantity):N2} ₽</td>
        </tr>
    "));

            var total = order.CatalogOrderproducts.Sum(i => i.Price * i.Quantity);

            return $@"
        <h2>Новый заказ №{order.Id}</h2>
        <p><strong>Дата:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
        <h3>Контактные данные</h3>
        <p><strong>Получатель:</strong> {order.Reciever}</p>
        <p><strong>Email:</strong> {order.Email}</p>
        <p><strong>Телефон:</strong> {order.Phone}</p>
        {(!string.IsNullOrEmpty(order.Comment) ? $"<p><strong>Комментарий:</strong> {order.Comment}</p>" : "")}
        <h3>Доставка и оплата</h3>
        <p><strong>Способ доставки:</strong> {order.Delivery?.Name}</p>
        <p><strong>Способ оплаты:</strong> {order.Payment?.Name}</p>
        <p><strong>Город доставки:</strong> {order.City?.Name}</p>
        {(order.Coupon != null ? $"<p><strong>Применён купон:</strong> {order.Coupon.Code}</p>" : "")}
        <h3>Состав заказа</h3>
        <table border='1' cellpadding='5' style='border-collapse: collapse;'>
            <thead>
                <tr>
                    <th>Товар</th>
                    <th>Цена</th>
                    <th>Кол-во</th>
                    <th>Сумма</th>
                </tr>
            </thead>
            <tbody>
                {itemsHtml}
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='3' align='right'><strong>Итого:</strong></td>
                    <td><strong>{total:N2} ₽</strong></td>
                </tr>
            </tfoot>
        </table>
    ";
        }
    }
}