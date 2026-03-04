namespace NBSite.Models.ViewComponents
{
    public class PriceRangeVM
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
