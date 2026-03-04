namespace NBSite.Areas.Admin.Models
{
    public class UserDetailsVM
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? CompanyPost { get; set; }
        public string? CityName { get; set; }
        public bool IsActive { get; set; }
        public bool IsStaff { get; set; }
        public bool IsSuperuser { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<UserOrderVM>? Orders { get; set; }
    }

    public class UserOrderVM
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public int StatusValue { get; set; }
        public double Total { get; set; }
        public int ItemsCount { get; set; }
    }
}
