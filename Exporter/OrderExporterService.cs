using DotNetDBF;
using Domain.Entities;
using FluentFTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Exporter
{
    public class OrderExporterService
    {
        private readonly NbshopContext _context;
        private readonly ILogger<OrderExporterService> _logger;
        private readonly FtpSettings _ftpSettings;

        public OrderExporterService(NbshopContext context, ILogger<OrderExporterService> logger, IOptions<FtpSettings> ftpSettings)
        {
            _context = context;
            _logger = logger;
            _ftpSettings = ftpSettings.Value;
        }

        public async Task ExportOrdersAsync()
        {
            var dateFrom = DateTime.UtcNow.AddDays(-30);
            var orders = await _context.CatalogOrders
                .Include(o => o.User).ThenInclude(u => u!.AccountsProfile)
                .Include(o => o.City)
                .Include(o => o.Delivery)
                .Include(o => o.Payment)
                .Include(o => o.CatalogOrderproducts)
                    .ThenInclude(op => op.Product)
                        .ThenInclude(p => p.Manufacturer)
                .Where(o => o.Status == 1 && o.SubmittedAt > dateFrom)
                .ToListAsync();

            _logger.LogInformation("Найдено {Count} заказов для экспорта", orders.Count);

            foreach (var order in orders)
            {
                try
                {
                    await ProcessOrderAsync(order);
                    order.Status = 2; // Отправлен
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Заказ #{OrderId} успешно обработан", order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке заказа #{OrderId}", order.Id);
                }
            }
        }

        private async Task ProcessOrderAsync(CatalogOrder order)
        {
            // Получаем код компании
            string companyNavCode = "12345"; // по умолчанию
            if (order.User?.AccountsProfile != null && !string.IsNullOrEmpty(order.User.AccountsProfile.CompanyNavCode))
            {
                companyNavCode = Regex.Replace(order.User.AccountsProfile.CompanyNavCode, @"\D", "");
                if (string.IsNullOrEmpty(companyNavCode)) companyNavCode = "12345";
            }

            string fileNameBase = $"sx{companyNavCode}_{order.Id}";
            string dbfFileName = $"{fileNameBase}.dbf";
            string zipFileName = $"{fileNameBase}.zip";
            string commentFileName = "dosmes.txt";

            string tempDir = _ftpSettings.LocalTempPath ?? Path.GetTempPath();
            string dbfPath = Path.Combine(tempDir, dbfFileName);
            string zipPath = Path.Combine(tempDir, zipFileName);
            string commentPath = Path.Combine(tempDir, $"{order.Id}-{commentFileName}");

            try
            {
                // Создаём DBF
                CreateDbf(order, dbfPath);

                // Создаём комментарий
                await CreateCommentFileAsync(order, commentPath);

                // Упаковываем в ZIP
                CreateZip(zipPath, dbfPath, commentPath, dbfFileName, commentFileName);

                // Отправляем на FTP
                await UploadToFtpAsync(zipFileName, zipPath);
            }
            finally
            {
                // Очистка временных файлов
                foreach (var file in new[] { dbfPath, zipPath, commentPath })
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }

        public void CreateDbf(CatalogOrder order, string dbfPath)
        {
            // 1. Определяем поля (полностью соответствуют вашим)
            var fields = new DBFField[]
            {
        new DBFField("SZ", NativeDbType.Char, 2),               // отсрочка платежа
        new DBFField("NZV", NativeDbType.Char, 8),              // номер заказа в системе клиента
        new DBFField("NFS", NativeDbType.Char, 8),              // код товара в нашей системе
        new DBFField("NMFS", NativeDbType.Char, 80),            // название товара
        new DBFField("ZIZG", NativeDbType.Char, 80),            // наименование производителя
        new DBFField("KOL", NativeDbType.Numeric, 10, 0),       // текущий остаток
        new DBFField("SER", NativeDbType.Char, 40),             // серия
        new DBFField("NDS", NativeDbType.Numeric, 10, 0),       // ставка НДС
        new DBFField("NP", NativeDbType.Numeric, 10, 0),        // не используется
        new DBFField("NZ", NativeDbType.Numeric, 6, 0),         // не используется
        new DBFField("TOPL", NativeDbType.Numeric, 12, 0),      // не используется
        new DBFField("UPAK", NativeDbType.Numeric, 15, 0),      // не используется
        new DBFField("SROK", NativeDbType.Char, 10),            // срок годности
        new DBFField("KOLZ", NativeDbType.Numeric, 10, 0),      // количество заказываемого товара
        new DBFField("ZN", NativeDbType.Numeric, 10, 2),        // цена с НДС
        new DBFField("SUM", NativeDbType.Numeric, 10, 2),       // сумма по позиции
        new DBFField("ORIGNOSZ", NativeDbType.Numeric, 19, 0)   // номер исходного заказа
            };

            // 2. Создаём писатель с указанием пути, полей и кодировки (Windows-1251)
            using (var writer = new DBFWriter(dbfPath))
            {
                // Устанавливаем поля
                writer.Fields = fields;
                // Устанавливаем кодировку (Windows-1251)
                writer.CharEncoding = Encoding.GetEncoding(1251);

                // 3. Перебираем товары и записываем каждую позицию
                foreach (var p in order.CatalogOrderproducts.Where(op => op.Quantity > 0))
                {
                    // Формируем массив значений строго по порядку полей
                    object[] record = new object[]
                    {
                "0",
                order.Id.ToString().PadLeft(8, '0'),
                p.ProductId.ToString(),
                p.Product?.Name ?? "",
                p.Product?.Manufacturer?.Name ?? "Не указан",
                (int)(p.Product?.Quantity ?? 0),
                "",
                0,
                0,
                0,
                0,
                0,
                "",
                (int)p.Quantity,
                p.Price,
                (p.Quantity * p.Price),
                order.Id
                    };

                    // Запись одной строки в файл
                    writer.WriteRecord(record);
                }
            } // writer.Dispose() автоматически закроет файл и обновит заголовок
        }

        private async Task CreateCommentFileAsync(CatalogOrder order, string commentPath)
        {
            var cityName = order.City?.Name ?? "-";
            var deliveryName = order.Delivery?.Name ?? "-";
            var paymentName = order.Payment?.Name ?? "-";
            var comment = CleanComment(order.Comment ?? "-");

            var lines = new[]
            {
                "Заявка с сайта",
                $"ФИО: {order.Reciever ?? "-"} | E-mail: {order.Email ?? "-"} | Телефон: {order.Phone ?? "-"}",
                $"Город: {cityName}",
                $"Способ доставки: {deliveryName}",
                $"Способ оплаты: {paymentName}",
                $"Комментарий: {comment}"
            };

            var content = string.Join("\r", lines);
            await File.WriteAllTextAsync(commentPath, content, Encoding.GetEncoding(866));
        }

        private string CleanComment(string comment)
        {
            var replacements = new Dictionary<string, string>
            {
                { "«", "\"" },
                { "»", "\"" }
            };
            foreach (var kv in replacements)
                comment = comment.Replace(kv.Key, kv.Value);
            return comment;
        }

        private void CreateZip(string zipPath, string dbfPath, string commentPath, string dbfEntryName, string commentEntryName)
        {
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(dbfPath, dbfEntryName);
                zip.CreateEntryFromFile(commentPath, commentEntryName);
            }
        }

        private async Task UploadToFtpAsync(string remoteFileName, string localFilePath)
        {
            using (var ftp = new AsyncFtpClient(_ftpSettings.Host, _ftpSettings.Username, _ftpSettings.Password))
            {
                await ftp.AutoConnect();
                await ftp.SetWorkingDirectory(_ftpSettings.RemoteDirectory);
                await ftp.UploadFile(localFilePath, remoteFileName);
            }
        }
    }
}