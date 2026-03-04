namespace NBSite.Models.ViewComponents
{
    public class ContactsViewModel
    {
        public List<ManagerViewModel> Managers { get; set; } = new();
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}
