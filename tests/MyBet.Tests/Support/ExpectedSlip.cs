using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public class ExpectedSlip
    {
        public string SlipId { get; set; } = "";
        public DateTime? PlacedAt { get; set; }
        public string? Type { get; set; }
        public decimal? Stake { get; set; }
        public decimal TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public decimal? BonusAmount { get; set; }
        public SlipStatus Status { get; set; }

        public List<ExpectedLeg> Legs { get; set; } = new();

        public Slip ToSlip() => new()
        {
            SlipId = SlipId,
            PlacedAt = PlacedAt,
            Type = Type,
            Stake = Stake,
            TotalOdds = TotalOdds,
            Payout = Payout,
            BonusPercent = BonusPercent,
            BonusAmount = BonusAmount,
            Status = Status,
            Legs = Legs.Select(leg => new Leg { Odds = leg.Odds, Result = leg.Result }).ToList()
        };
    }
}