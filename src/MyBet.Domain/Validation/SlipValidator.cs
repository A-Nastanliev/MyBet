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

            if (slip.Status is null)
                issues.Add("Slip status not determined.");

            var undetermined = slip.Legs.Count(l => l.Result is null);
            var pending = slip.Legs.Count(l => l.Result == LegResult.Pending);
            var anyLost = slip.Legs.Any(l => l.Result == LegResult.Lost);

            if (undetermined > 0)
                issues.Add($"{undetermined} leg result(s) not determined.");

            if (pending > 0 && slip.Status != SlipStatus.CashedOut
                && !(slip.Status == SlipStatus.Lost && anyLost))
                issues.Add($"{pending} pending leg(s) but the slip is not Lost via a lost leg or cashed out.");

            if (slip.Status == SlipStatus.CashedOut && anyLost)
                issues.Add("Slip is CashedOut but a leg is Lost.");

            if (slip.Status == SlipStatus.Won && anyLost)
                issues.Add("Slip is Won but a leg is Lost.");

            if (slip.Status == SlipStatus.Lost && !anyLost)
                issues.Add("Slip is Lost but no leg is Lost.");

            if (slip.Status == SlipStatus.Won && slip.Stake is { } stake && slip.Payout is { } payout)
            {
                var gross = stake * slip.TotalOdds;
                var expected = gross + (slip.BonusPercent ?? 0m) / 100m * (gross - stake);
                if (Math.Abs(payout - expected) > MoneyTolerance + stake * 0.005m)
                    issues.Add($"Payout {payout:F2} != expected {expected:F2}.");
            }

            return issues;
        }
    }
}