namespace MyBet.Domain.Entities
{
    public class ReviewItem
    {
        public int Id { get; set; }
        public string FileHash { get; set; } = "";
        public string FilePath { get; set; } = "";
        public ExtractedSlip Extracted { get; set; } = new();
    }
}
