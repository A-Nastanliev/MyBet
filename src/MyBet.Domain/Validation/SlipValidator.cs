using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Domain.Validation
{
    public static class SlipValidator
    {
        private const decimal OddsTolerance = 0.01m;
        private const decimal MoneyTolerance = 0.02m;

        public static IReadOnlyList<string> Validate(Slip slip)
        {
            var issues = new List<string>();

            if (slip.Legs.Count == 0)
            {
                issues.Add("No legs extracted.");
                return issues;
            }

            var product = slip.Legs.Aggregate(1m, (acc, l) => acc * l.Odds);
            if (Math.Abs(product - slip.TotalOdds) > slip.TotalOdds * OddsTolerance)
                issues.Add($"Odds product {product:F2} != total odds {slip.TotalOdds:F2}.");

            if (slip.Stake is { } stake && slip.Payout is { } payout && slip.Status == SlipStatus.Won)
            {
                var gross = stake * slip.TotalOdds;
                var bonus = (slip.BonusPercent ?? 0) / 100m * (gross - stake);
                var expected = gross + bonus;
                if (Math.Abs(expected - payout) > MoneyTolerance)
                    issues.Add($"Payout {payout:F2} != expected {expected:F2}.");
            }

            return issues;
        }
    }
}
