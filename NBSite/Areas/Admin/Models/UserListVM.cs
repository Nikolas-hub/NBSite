namespace NBSite.Areas.Admin.Models
{
    public class UserListVM
    {
        public List<UserListItemVM>? Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchQuery { get; set; }
    }
}
