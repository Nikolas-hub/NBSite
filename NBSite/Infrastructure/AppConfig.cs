namespace NBSite.Infrastructure
{
    public class AppConfig
    {
        public Company Company { get; set; } = new Company();
        public Database Database { get; set; } = new Database();
        public FooterSettings Footer { get; set; } = new FooterSettings();
        public SmtpSettings Smtp { get; set; } = new SmtpSettings();
        public SiteSettings Site {  get; set; } = new SiteSettings();
    }

    public class Database
    {
        public string? ConnectionString { get; set; }
    }

    public class Company
    {
        public string? CompanyName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyPhoneShort { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyTelegram { get; set; }
    }
    public class FooterSettings
    {
        public string? Address { get; set; }
        public int FoundingYear { get; set; }
        public string? LogoPath { get; set; }
    }
    public class SmtpSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? From { get; set; }
        public string? FromName { get; set; }
    }
    public class SiteSettings
    {
        public string? Url { get; set; }
    }
}

