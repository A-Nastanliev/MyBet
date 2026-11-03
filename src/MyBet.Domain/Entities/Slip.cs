using MyBet.Domain.Enums;

namespace MyBet.Domain.Entities
{
    public class Slip
    {
        public int Id { get; set; }
        public string SlipId { get; set; } = "";
        public DateTime? PlacedAt { get; set; }
        public string? Type { get; set; }
        public decimal? Stake { get; set; }
        public decimal TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public decimal? BonusAmount { get; set; }
        public SlipStatus? Status { get; set; }
        public bool NeedsReview { get; set; }
        public List<Leg> Legs { get; set; } = new();
    }
}