using FluentFTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Importer
{
    public class FtpHelper
    {
        private readonly FtpSettings _settings;
        private readonly ILogger<FtpHelper> _logger;

        public FtpHelper(IOptions<FtpSettings> settings, ILogger<FtpHelper> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task DownloadFileAsync(string remoteFileName, string localFilePath)
        {
            using var ftp = new AsyncFtpClient(_settings.Host, _settings.Username, _settings.Password);

            // Отключаем шифрование (сервер не поддерживает TLS)
            ftp.Config.EncryptionMode = FtpEncryptionMode.None;

            // Рекомендуется использовать пассивный режим для работы за брандмауэрами
            ftp.Config.DataConnectionType = FtpDataConnectionType.PASV;

            // Можно увеличить таймауты, если сеть медленная (значения в миллисекундах)
            ftp.Config.ConnectTimeout = 10000; // 10 секунд
            ftp.Config.ReadTimeout = 10000;    // 10 секунд

            // Вместо AutoConnect используем обычный Connect, т.к. мы уже задали режим
            await ftp.Connect();

            await ftp.SetWorkingDirectory(_settings.RemoteDirectory);

            // Удаляем локальный файл, если существует
            if (File.Exists(localFilePath))
                File.Delete(localFilePath);

            var localDir = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(localDir))
                Directory.CreateDirectory(localDir!);

            var status = await ftp.DownloadFile(localFilePath, remoteFileName);
            if (status == FtpStatus.Failed)
                throw new Exception($"Failed to download {remoteFileName}");

            _logger.LogInformation("Downloaded {RemoteFile} -> {LocalFile}", remoteFileName, localFilePath);
        }
    }
}
