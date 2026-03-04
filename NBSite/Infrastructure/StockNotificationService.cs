using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace NBSite.Infrastructure
{
    public class StockNotificationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<StockNotificationService> _logger;

        public StockNotificationService(IServiceProvider services, ILogger<StockNotificationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSubscriptionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке подписок на товары");
                }

                // Интервал проверки – 5 минут (можно настроить)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task ProcessSubscriptionsAsync(CancellationToken stoppingToken)
        {
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NbshopContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var appConfig = scope.ServiceProvider.GetRequiredService<AppConfig>();

                // Находим все активные подписки (Status = 0) для товаров, которые есть в наличии
                var subscriptions = await db.CatalogInstocksubscriptions
                    .Include(s => s.Product)
                    .Where(s => s.Status == 0 && s.Product.Quantity > 0)
                    .ToListAsync(stoppingToken);

                if (!subscriptions.Any())
                    return;

                // Группируем по товару, чтобы отправлять одно письмо на email (если подписок несколько – не дублируем)
                var grouped = subscriptions.GroupBy(s => s.ProductId);

                foreach (var group in grouped)
                {
                    var product = group.First().Product;
                    var emails = group.Select(s => s.Email).Distinct().ToList();

                    // Отправляем письма всем подписчикам этого товара
                    foreach (var email in emails)
                    {
                        try
                        {
                            var subject = $"Товар {product.Name} появился в наличии";
                            var body = $@"
                            <h2>Здравствуйте!</h2>
                            <p>Товар <strong>{product.Name}</strong> снова появился в наличии.</p>
                            <p>Вы можете приобрести его по ссылке:</p>
                            <p><a href='{appConfig.Site.Url}/shopping/product/{product.Alias}'>{appConfig.Site.Url}/shopping/product/{product.Alias}</a></p>
                            <p>С уважением, интернет-магазин.</p>
                        ";

                            await emailSender.SendEmailAsync(email, subject, body, true);
                            _logger.LogInformation("Уведомление отправлено на {Email} для товара {ProductId}", email, product.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка отправки уведомления на {Email} для товара {ProductId}", email, product.Id);
                            // Продолжаем для других email, не прерываем всю группу
                        }
                    }

                    // Помечаем все подписки этого товара как обработанные (статус = 1)
                    var subscriptionIds = group.Select(s => s.Id).ToList();
                    var subsToUpdate = await db.CatalogInstocksubscriptions
                        .Where(s => subscriptionIds.Contains(s.Id))
                        .ToListAsync(stoppingToken);

                    foreach (var sub in subsToUpdate)
                    {
                        sub.Status = 1; // Отправлено
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
