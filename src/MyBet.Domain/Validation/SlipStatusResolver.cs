using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Domain.Validation
{
    public static class SlipStatusResolver
    {
        private const decimal MoneyTolerance = 0.02m;
        private const decimal StakeTolerance = 0.005m;

        public static SlipStatus? Resolve(ExtractedSlip slip)
        {
            if (slip.Stake is not { } stake || slip.TotalOdds is not { } odds)
                return null;

            var payout = slip.Payout ?? 0m;
            if (payout == 0m) return SlipStatus.Lost;

            var gross = stake * odds;
            var expected = gross + (slip.BonusPercent ?? 0m) / 100m * (gross - stake);
            var tolerance = MoneyTolerance + stake * StakeTolerance;

            if (Math.Abs(payout - expected) <= tolerance) return SlipStatus.Won;
            if (payout < expected) return SlipStatus.CashedOut;
            return null;
        }
    }
}
