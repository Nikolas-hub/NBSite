namespace NBSite.Models.ViewComponents
{
    public class NewsPreviewVM
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string? IntroText { get; set; }
        public string? Image { get; set; }

        public string FormattedDate => Date.ToString("dd.MM.yyyy");
        public string? Thumbnail => !string.IsNullOrEmpty(Image) ? $"{Image}?size=300x200" : null;
    }
}
