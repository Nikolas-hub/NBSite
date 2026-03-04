using Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NBSite.Infrastructure;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;

namespace NBSite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Подключение конфигурации

            AppConfig config = builder.Configuration.GetSection("Project").Get<AppConfig>()!;
            builder.Services.AddSingleton(config);

            // Подключение контекста БД PostgreSQL
            builder.Services.AddDbContext<NbshopContext>(options =>
                options.UseNpgsql(
                    config.Database.ConnectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(NbshopContext).Assembly.FullName);
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    })
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            // Регистрация сервисов (ОДИН РАЗ!)
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ICategoryMenuService, CategoryMenuService>();
            builder.Services.AddHostedService<StockNotificationService>();
            // Регистрация сервиса отправки почты
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Настройка аутентификации с помощью Cookie (без Identity)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "NBSiteAuth";
                    options.Cookie.HttpOnly = true;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

            // Настройка авторизации
            // Теперь используем IsStaff и IsSuperuser вместо ролей
            builder.Services.AddAuthorization(options =>
            {
                // Для обычных пользователей - достаточно быть активным
                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role &&
                            (c.Value == "user" || c.Value == "admin" || c.Value == "superuser")) ||
                        context.User.HasClaim("IsActive", "true")));

                // Для администраторов - требуется IsStaff = true
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role &&
                            (c.Value == "admin" || c.Value == "superuser")) ||
                        context.User.HasClaim("IsStaff", "true")));

                // Для суперпользователей - требуется IsSuperuser = true
                options.AddPolicy("SuperuserPolicy", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role &&
                            c.Value == "superuser") ||
                        context.User.HasClaim("IsSuperuser", "true")));
            });

            // Подключение контроллеров
            builder.Services.AddControllersWithViews();

            // Добавляем поддержку сессий 
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(7);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Сборка конфигурации
            WebApplication app = builder.Build();

            // Настройка Forwarded Headers для работы за прокси (nginx)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                // Очищаем KnownIPNetworks/KnownProxies, чтобы доверять всем прокси в контейнерной сети.
                // В изолированной среде это безопасно. Если нужно строже – укажите подсеть Docker.
                KnownIPNetworks = { },
                KnownProxies = { }
            });

            // Конфигурация конвейера HTTP-запросов
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            var defaultCulture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(defaultCulture),
                SupportedCultures = new List<CultureInfo> { defaultCulture },
                SupportedUICultures = new List<CultureInfo> { defaultCulture }
            });

            // Добавляем поддержку сессий
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // Маршрутизация
            app.MapControllerRoute(
                name: "product",
                pattern: "Shopping/ShowProduct/{id}",
                defaults: new { controller = "Shopping", action = "ShowProduct" });

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");





            // Инициализация базы данных при первом запуске
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    var context = serviceProvider.GetRequiredService<NbshopContext>();
                    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

                    // Применяем миграции автоматически
                    await context.Database.MigrateAsync();

                    // Проверяем, есть ли администратор
                    var adminUser = await context.AuthUsers
                        .FirstOrDefaultAsync(u => u.Email == "admin@example.com" || u.Username == "admin");

                    if (adminUser == null)
                    {
                        // Создаем администратора с использованием passwordHasher
                        adminUser = new AuthUser
                        {
                            Username = "admin",
                            Email = "admin@example.com",
                            Password = passwordHasher.HashPassword("Admin123!"),
                            IsActive = true,
                            IsStaff = true,
                            IsSuperuser = true,
                            DateJoined = DateTime.UtcNow,
                            FirstName = "Admin",
                            LastName = "User"
                        };

                        context.AuthUsers.Add(adminUser);
                        await context.SaveChangesAsync();

                        // Создаем профиль для администратора
                        var profile = new AccountsProfile
                        {
                            UserId = adminUser.Id,
                            Email = adminUser.Email,
                            Fio = "Администратор",
                            PricesVisible = true
                        };

                        context.AccountsProfiles.Add(profile);
                        await context.SaveChangesAsync();
                    }

                    // Проверяем, есть ли необходимые способы оплаты и доставки
                    if (!await context.CatalogPayments.AnyAsync())
                    {
                        var payments = new[]
                        {
                            new CatalogPayment { Name = "Наличные при получении", Handler = "cash_on_delivery" },
                            new CatalogPayment { Name = "Банковской картой онлайн", Handler = "online_card" },
                            new CatalogPayment { Name = "Безналичный расчет", Handler = "bank_transfer" }
                        };
                        context.CatalogPayments.AddRange(payments);
                        await context.SaveChangesAsync();
                    }

                    if (!await context.CatalogDeliveries.AnyAsync())
                    {
                        var deliveries = new[]
                        {
                            new CatalogDelivery { Name = "Самовывоз", Handler = "pickup", Sort = 100 },
                            new CatalogDelivery { Name = "Курьерская доставка", Handler = "courier", Sort = 200 },
                            new CatalogDelivery { Name = "Почта России", Handler = "russian_post", Sort = 300 },
                            new CatalogDelivery { Name = "Транспортная компания", Handler = "transport_company", Sort = 400 }
                        };
                        context.CatalogDeliveries.AddRange(deliveries);
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ошибка при инициализации базы данных");

                    // В режиме разработки выводим полную ошибку
                    if (app.Environment.IsDevelopment())
                    {
                        throw;
                    }
                }
            }

            app.Run();
        }
    }
}