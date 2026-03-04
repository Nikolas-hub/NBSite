namespace NBSite.Areas.Admin.Models
{
    public class ManufacturerListVM
    {
        public List<ManufacturerListItemVM>? Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchQuery { get; set; }
    }
}
