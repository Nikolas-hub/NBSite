namespace NBSite.Models
{
    public class CategoryFilterDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ParentCode { get; set; }
        public bool Selected { get; set; }
        public List<CategoryFilterDto> Children { get; set; } = new();
    }
}
