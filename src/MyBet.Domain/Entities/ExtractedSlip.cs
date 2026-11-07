using MyBet.Domain.Enums;

namespace MyBet.Domain.Entities
{
    public class ExtractedSlip
    {
        public string SlipId { get; set; } = "";
        public decimal? Stake { get; set; }
        public decimal? TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public SlipStatus? Status { get; set; }

        public bool NeedsReview => Status is null || SlipId.Length == 0;

        public Slip ToSlip()
        {
            if (NeedsReview || Stake is not { } stake || TotalOdds is not { } odds || Status is not { } status)
                throw new InvalidOperationException("This slip needs review and cannot be saved as it is.");

            return new Slip
            {
                SlipId = SlipId,
                Stake = stake,
                TotalOdds = odds,
                Payout = Payout ?? 0m,
                BonusPercent = BonusPercent,
                Status = status
            };
        }
    }
}
