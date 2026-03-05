using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Importer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("NbshopDatabase");
                    services.AddDbContext<NbshopContext>(options =>
                        options.UseNpgsql(connectionString));

                    services.Configure<FtpSettings>(context.Configuration.GetSection("FtpSettings"));
                    services.AddSingleton<FtpHelper>();
                    services.AddScoped<DataImporter>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                    // Явно для EF Core, если вдруг не сработает общий
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                })
                .Build();

            // Определяем, какой импорт запустить
            var importer = host.Services.GetRequiredService<DataImporter>();
            var argsList = args.ToList();

            if (argsList.Contains("categories") || argsList.Count == 0)
                await importer.ImportCategoriesAsync();
            if (argsList.Contains("manufacturers") || argsList.Count == 0)
                await importer.ImportManufacturersAsync();
            if (argsList.Contains("products") || argsList.Count == 0)
                await importer.ImportProductsAsync();
            if (argsList.Contains("relations") || argsList.Count == 0)
                await importer.ImportCategoryRelationsAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("All imports completed.");
        }
    }
}
