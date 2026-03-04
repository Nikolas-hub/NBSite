namespace NBSite.Areas.Admin.Models
{
    public class UserListItemVM
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsStaff { get; set; }
        public bool IsSuperuser { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime? LastLogin { get; set; }
        public int OrdersCount { get; set; }
    }
}
