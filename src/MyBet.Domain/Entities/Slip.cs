using MyBet.Domain.Enums;

namespace MyBet.Domain.Entities
{
    public class Slip
    {
        public int Id { get; set; }
        public string SlipId { get; set; } = "";
        public decimal? Stake { get; set; }
        public decimal? TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public SlipStatus? Status { get; set; }
        public bool NeedsReview { get; set; }
    }
}