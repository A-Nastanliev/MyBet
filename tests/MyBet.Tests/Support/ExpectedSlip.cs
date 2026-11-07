using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public class ExpectedSlip
    {
        public string SlipId { get; set; } = "";
        public decimal? Stake { get; set; }
        public decimal? TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public SlipStatus Status { get; set; }

        public Slip ToSlip() => new()
        {
            SlipId = SlipId,
            Stake = Stake,
            TotalOdds = TotalOdds,
            Payout = Payout,
            BonusPercent = BonusPercent,
            Status = Status
        };
    }
}