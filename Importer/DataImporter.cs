using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Xml.Linq;

namespace Importer
{
    public class DataImporter
    {
        private readonly FtpHelper _ftp;
        private readonly ILogger<DataImporter> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;

        public DataImporter(
            FtpHelper ftp,
            ILogger<DataImporter> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _ftp = ftp;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;
        }

        private string GetLocalFilePath(string fileName)
        {
            var tempPath = _config["FtpSettings:LocalTempPath"] ?? "/tmp";
            var prefix = _config["Import:FilePrefix"] ?? "nb_import_";
            return Path.Combine(tempPath, prefix + fileName);
        }

        public async Task ImportCategoriesAsync()
        {
            const string fileName = "categories.xml";
            var filePath = GetLocalFilePath(fileName);
            await _ftp.DownloadFileAsync(fileName, filePath);

            var doc = XDocument.Load(filePath);
            var categories = doc.Descendants("category");

            var fieldMap = new Dictionary<string, string>
            {
                ["code"] = "code",
                ["name"] = "name",
                ["parent"] = "parent"
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NbshopContext>();
            using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var xmlCat in categories)
            {
                var fields = FillFields(xmlCat, fieldMap);

                var code = fields["code"]?.ToString();
                var name = fields["name"]?.ToString();
                var parent = fields["parent"]?.ToString();

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
                    continue; // пропускаем, если нет обязательных полей

                if (string.IsNullOrEmpty(parent))
                    parent = null;

                var existing = await context.CatalogCategories
                    .FirstOrDefaultAsync(c => c.Code == code);

                if (existing == null)
                {
                    existing = new CatalogCategory();
                    context.CatalogCategories.Add(existing);
                }

                existing.Code = code;
                existing.Name = name;
                existing.Parent = parent;
                if (string.IsNullOrEmpty(existing.Alias))
                {
                    existing.Alias = code; // или можно использовать name с транслитерацией
                }


                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving category {Code}", code);
                }
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Categories import completed.");
        }

        public async Task ImportManufacturersAsync()
        {
            const string fileName = "manufacturers.xml";
            var filePath = GetLocalFilePath(fileName);
            await _ftp.DownloadFileAsync(fileName, filePath);

            var doc = XDocument.Load(filePath);
            var manufacturers = doc.Descendants("manufacturer");

            var fieldMap = new Dictionary<string, string>
            {
                ["code"] = "code",
                ["name"] = "name"
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NbshopContext>();
            using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var xmlMan in manufacturers)
            {
                var fields = FillFields(xmlMan, fieldMap);

                var code = fields["code"]?.ToString();
                var name = fields["name"]?.ToString();

                if (string.IsNullOrEmpty(name))
                    continue;

                if (string.IsNullOrEmpty(code))
                    continue; // код должен быть

                var existing = await context.CatalogManufacturers
                    .FirstOrDefaultAsync(m => m.Code == code);

                if (existing == null)
                {
                    existing = new CatalogManufacturer();
                    context.CatalogManufacturers.Add(existing);
                }

                existing.Code = code;
                existing.Name = name;
                if (string.IsNullOrEmpty(existing.Alias))
                {
                    existing.Alias = code; // или можно использовать name с транслитерацией
                }


                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving manufacturer {Code}", code);
                }
            }

            await transaction.CommitAsync();
        }

        public async Task ImportProductsAsync()
        {
            const string fileName = "items.xml";
            var filePath = GetLocalFilePath(fileName);
            await _ftp.DownloadFileAsync(fileName, filePath);

            var doc = XDocument.Load(filePath);
            var xmlProducts = doc.Descendants("item");

            var fieldMap = new Dictionary<string, string>
            {
                ["id"] = "no",
                ["name"] = "name",
                ["price"] = "priceVAT",
                ["quantity"] = "qtyOnStock",
                ["content"] = "description",
                ["ean13"] = "EAN13",
                ["volume"] = "volume",
                ["weight"] = "weight",
                ["meta_keywords"] = "keyWords",
                ["multiplicity"] = "multiplicity",
                ["popular"] = "masthead",
                ["expiration_date"] = "workingLife",
                ["old_price"] = "oldPrice",
                ["has_reject"] = "hasReject"
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NbshopContext>();

            // Загружаем справочники кодов -> ID
            var categoriesCodesToIds = await context.CatalogCategories
                .Where(c => c.Code != null)
                .ToDictionaryAsync(c => c.Code!, c => c.Id);

            var manufacturersCodesToIds = await context.CatalogManufacturers
                .Where(m => m.Code != null)
                .ToDictionaryAsync(m => m.Code!, m => m.Id);

            // Для back-in-stock
            var zeroQuantityProductIds = await context.CatalogProducts
                .Where(p => p.Quantity == 0)
                .Select(p => p.Id)
                .ToListAsync();

            var backInStockProductIds = new List<long>();

            using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var xmlProduct in xmlProducts)
            {
                var fields = FillFields(xmlProduct, fieldMap);

                FixProductFields(fields);

                // Определяем category_id
                var categoryCode = xmlProduct.Element("category")?.Value;
                if (string.IsNullOrEmpty(categoryCode) || !categoriesCodesToIds.TryGetValue(categoryCode, out var categoryId))
                    continue; // пропускаем, как в Python

                fields["category_id"] = categoryId;

                // Активность
                var blocked = xmlProduct.Element("blocked")?.Value == "true";
                var notInUse = xmlProduct.Element("notInUse")?.Value == "true";
                fields["active"] = !blocked && !notInUse;

                // manufacturer_id
                var manufacturerCode = xmlProduct.Element("manufacturerCode")?.Value;
                if (!string.IsNullOrEmpty(manufacturerCode) && manufacturersCodesToIds.TryGetValue(manufacturerCode, out var manufacturerId))
                    fields["manufacturer_id"] = manufacturerId;
                else
                    fields["manufacturer_id"] = null!;

                // Пытаемся получить товар по ID
                var productId = long.Parse(fields["id"].ToString()!);
                var product = await context.CatalogProducts.FindAsync(productId);

                bool created = false;
                if (product == null)
                {
                    product = new CatalogProduct { Id = productId };
                    context.CatalogProducts.Add(product);
                    created = true;
                }

                if (created)
                {
                    PrepareNewProduct(product, fields);
                }
                else
                {
                    if (zeroQuantityProductIds.Contains(productId) && (decimal)fields["quantity"] > 0)
                    {
                        backInStockProductIds.Add(productId);
                    }
                    PrepareExistingProduct(product, fields);
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving product {Id}", productId);
                }
            }

            await transaction.CommitAsync();

            // Обновляем подписки
            if (backInStockProductIds.Any())
            {
                await UpdateBackInStockSubscriptionsAsync(backInStockProductIds);
            }
        }

        private void FixProductFields(Dictionary<string, object> fields)
        {
            // Преобразование числовых полей (могут содержать пробелы, запятые)
            var numericFields = new[] { "price", "weight", "volume", "quantity", "old_price" };
            foreach (var key in numericFields)
            {
                if (fields.TryGetValue(key, out var val) && val is string str && !string.IsNullOrWhiteSpace(str))
                {
                    str = str.Replace(" ", "").Replace(",", ".").Replace("\u00A0", "");
                    if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                        fields[key] = dec;
                }
            }

            // Целочисленные
            if (fields.TryGetValue("id", out var idVal) && idVal is string idStr)
                fields["id"] = long.Parse(idStr);
            if (fields.TryGetValue("multiplicity", out var multVal))
            {
                if (multVal is string multStr)
                {
                    if (int.TryParse(multStr, out var mult))
                        fields["multiplicity"] = mult == 0 ? 1 : mult;
                    else
                        fields["multiplicity"] = 1;
                }
            }

            // Boolean
            if (fields.TryGetValue("popular", out var popVal))
                fields["popular"] = popVal?.ToString() == "true";
            if (fields.TryGetValue("has_reject", out var rejVal))
                fields["has_reject"] = rejVal?.ToString() == "true";

            // Дата
            if (fields.TryGetValue("expiration_date", out var expVal) && expVal is string expStr)
            {
                if (DateTime.TryParseExact(expStr, "dd.MM.yy", null, System.Globalization.DateTimeStyles.None, out var date))
                    fields["expiration_date"] = DateOnly.FromDateTime(date);
                else
                    fields["expiration_date"] = null!;
            }
        }

        private void PrepareNewProduct(CatalogProduct product, Dictionary<string, object> fields)
        {
            foreach (var kv in fields)
            {
                switch (kv.Key)
                {
                    case "id":
                        product.Id = Convert.ToInt64(kv.Value);
                        break;
                    case "name":
                        product.Name = Convert.ToString(kv.Value)!;
                        break;
                    case "price":
                        product.Price = Convert.ToDouble(kv.Value);
                        break;
                    case "quantity":
                        product.Quantity = Convert.ToInt32(kv.Value);
                        break;
                    case "content":
                        product.Content = Convert.ToString(kv.Value);
                        break;
                    case "ean13":
                        product.Ean13 = kv.Value?.ToString(); // как в PrepareExistingProduct
                        break;
                    case "volume":
                        product.Volume = Convert.ToDouble(kv.Value);
                        break;
                    case "weight":
                        product.Weight = Convert.ToDouble(kv.Value);
                        break;
                    case "meta_keywords":
                        product.MetaKeywords = Convert.ToString(kv.Value);
                        break;
                    case "multiplicity":
                        product.Multiplicity = Convert.ToInt32(kv.Value);
                        break;
                    case "popular":
                        product.Popular = Convert.ToBoolean(kv.Value);
                        break;
                    case "expiration_date":
                        product.ExpirationDate = kv.Value == null ? null : DateOnly.FromDateTime(Convert.ToDateTime(kv.Value));
                        break;
                    case "old_price":
                        product.OldPrice = Convert.ToDouble(kv.Value);
                        break;
                    case "has_reject":
                        product.HasReject = Convert.ToBoolean(kv.Value);
                        break;
                    case "category_id":
                        product.CategoryId = kv.Value == null ? null : Convert.ToInt64(kv.Value);
                        break;
                    case "active":
                        product.Active = Convert.ToBoolean(kv.Value);
                        break;
                    case "manufacturer_id":
                        product.ManufacturerId = kv.Value == null ? null : Convert.ToInt64(kv.Value);
                        break;
                    default:
                        break;
                }
            }
            // Если после всех присваиваний alias остался null, подставляем Id
            if (string.IsNullOrEmpty(product.Alias))
            {
                product.Alias = product.Id.ToString();
            }
        }

        private void PrepareExistingProduct(CatalogProduct product, Dictionary<string, object> fields)
        {
            var allowed = new[] { "active", "price", "weight", "category_id", "volume", "ean13", "quantity", "manufacturer_id", "popular", "expiration_date" };
            foreach (var key in allowed)
            {
                if (fields.TryGetValue(key, out var val))
                {
                    switch (key)
                    {
                        case "active":
                            product.Active = Convert.ToBoolean(val);
                            break;
                        case "price":
                            product.Price = Convert.ToDouble(val);
                            break;
                        case "weight":
                            product.Weight = Convert.ToDouble(val);
                            break;
                        case "category_id":
                            product.CategoryId = val == null ? null : Convert.ToInt64(val);
                            break;
                        case "volume":
                            product.Volume = Convert.ToDouble(val);
                            break;
                        case "ean13":
                            product.Ean13 = val?.ToString();
                            break;
                        case "quantity":
                            product.Quantity = Convert.ToInt32(val);
                            break;
                        case "manufacturer_id":
                            product.ManufacturerId = val == null ? null : Convert.ToInt64(val);
                            break;
                        case "popular":
                            product.Popular = Convert.ToBoolean(val);
                            break;
                        case "expiration_date":
                            // Если приходит строка или DateTime, используйте парсинг
                            product.ExpirationDate = val == null ? null : DateOnly.FromDateTime(Convert.ToDateTime(val));
                            break;
                    }
                }
            }
            // Аналогично для существующего продукта: если alias не задан, используем Id
            if (string.IsNullOrEmpty(product.Alias))
            {
                product.Alias = product.Id.ToString();
            }
        }

        private async Task UpdateBackInStockSubscriptionsAsync(List<long> productIds)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NbshopContext>();
            var subscriptions = context.CatalogInstocksubscriptions
                .Where(s => productIds.Contains(s.ProductId) && s.Status == 0);
            foreach (var sub in subscriptions)
            {
                sub.Status = 1;
            }
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated {Count} back-in-stock subscriptions", productIds.Count);
        }

        public async Task ImportCategoryRelationsAsync()
        {
            const string fileName = "upselling.xml";
            var filePath = GetLocalFilePath(fileName);
            await _ftp.DownloadFileAsync(fileName, filePath);

            var doc = XDocument.Load(filePath);
            var categoryNodes = doc.Descendants("category"); // XPath: ./categories/category

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NbshopContext>();
            using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var xmlCat in categoryNodes)
            {
                var code = xmlCat.Element("code")?.Value;
                if (string.IsNullOrEmpty(code))
                    continue;

                var category = await context.CatalogCategories.FirstOrDefaultAsync(c => c.Code == code);
                if (category == null)
                    continue;

                var relatedCodes = xmlCat.Descendants("code") // отбираем коды внутри relatedCategories/code
                    .Select(e => e.Value).ToList();

                // Удаляем неактуальные связи
                var existingRelations = context.CatalogCategoryrelations
                    .Where(r => r.SourceId == category.Id);
                var toDelete = existingRelations
                    .Where(r => !relatedCodes.Contains(r.Target.Code!));
                context.CatalogCategoryrelations.RemoveRange(toDelete);

                // Создаём новые
                foreach (var relCode in relatedCodes)
                {
                    var target = await context.CatalogCategories.FirstOrDefaultAsync(c => c.Code == relCode);
                    if (target == null) continue;

                    var exists = await context.CatalogCategoryrelations
                        .AnyAsync(r => r.SourceId == category.Id && r.TargetId == target.Id);
                    if (!exists)
                    {
                        context.CatalogCategoryrelations.Add(new CatalogCategoryrelation
                        {
                            SourceId = category.Id,
                            TargetId = target.Id,
                            Type = 0 // BUY_WITH_IT, в Python используется CategoryRelation.BUY_WITH_IT, предположим 0
                        });
                    }
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing relations for category {Code}", code);
                }
            }

            await transaction.CommitAsync();
        }


        private Dictionary<string, object> FillFields(XElement xmlEntry, Dictionary<string, string> fieldMap)
        {
            var result = new Dictionary<string, object>();
            foreach (var kv in fieldMap)
            {
                var element = xmlEntry.Element(kv.Value);
                result[kv.Key] = element?.Value!; // если элемент отсутствует, будет null
            }
            return result;
        }
    }
}
