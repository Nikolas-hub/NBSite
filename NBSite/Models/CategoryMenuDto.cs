namespace NBSite.Models
{
    public class CategoryMenuDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Code { get; set; }
        public string? ParentCode { get; set; }  
        public bool Active { get; set; }
        public List<CategoryMenuDto> Children { get; set; } = new();
    }
}
