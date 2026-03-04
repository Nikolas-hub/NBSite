using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Exporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
                    services.AddScoped<OrderExporterService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            var exporter = host.Services.GetRequiredService<OrderExporterService>();
            await exporter.ExportOrdersAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Экспорт заказов завершён.");
        }
    }
}