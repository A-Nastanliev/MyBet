namespace MyBet.Persistence.Models
{
    public class ReviewItemRecord
    {
        public int Id { get; set; }
        public string FileHash { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string SlipId { get; set; } = "";
        public decimal? Stake { get; set; }
        public decimal? TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
    }
}
