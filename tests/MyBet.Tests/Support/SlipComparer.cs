using MyBet.Domain.Entities;

namespace MyBet.Tests.Support
{
    public static class SlipComparer
    {
        public const double TextThreshold = 0.75;

        public static IReadOnlyList<string> Compare(ExpectedSlip expected, Slip actual)
        {
            var diffs = new List<string>();

            ExactText(diffs, "slipId", expected.SlipId, actual.SlipId);
            Exact(diffs, "isLive", expected.IsLive, actual.IsLive);
            Exact(diffs, "placedAt", expected.PlacedAt, actual.PlacedAt);
            ExactText(diffs, "type", expected.Type, actual.Type);
            Exact(diffs, "stake", expected.Stake, actual.Stake);
            Exact(diffs, "totalOdds", expected.TotalOdds, actual.TotalOdds);
            Exact(diffs, "payout", expected.Payout, actual.Payout);
            Exact(diffs, "bonusPercent", expected.BonusPercent, actual.BonusPercent);
            Exact(diffs, "bonusAmount", expected.BonusAmount, actual.BonusAmount);

            if (expected.Legs.Count != actual.Legs.Count)
            {
                diffs.Add($"legs: expected {expected.Legs.Count}, got {actual.Legs.Count} (leg-by-leg comparison skipped)");
                return diffs;
            }

            for (var i = 0; i < expected.Legs.Count; i++)
                CompareLeg(diffs, $"leg {i + 1}", expected.Legs[i], actual.Legs[i]);

            return diffs;
        }

        private static void CompareLeg(List<string> diffs, string label, ExpectedLeg expected, Leg actual)
        {
            Similar(diffs, $"{label} homeTeam", expected.HomeTeam, actual.HomeTeam);
            Similar(diffs, $"{label} awayTeam", expected.AwayTeam, actual.AwayTeam);
            Similar(diffs, $"{label} market", expected.Market, actual.Market);
            Similar(diffs, $"{label} selection", expected.Selection, actual.Selection);
            Exact(diffs, $"{label} odds", expected.Odds, actual.Odds);
            ExactText(diffs, $"{label} score", Compact(expected.Score), Compact(actual.Score));
            Exact(diffs, $"{label} isBetBuilder", expected.IsBetBuilder, actual.IsBetBuilder);

            if (expected.SubSelections.Count != actual.SubSelections.Count)
            {
                diffs.Add($"{label} subSelections: expected {expected.SubSelections.Count}, got {actual.SubSelections.Count}");
                return;
            }

            for (var i = 0; i < expected.SubSelections.Count; i++)
            {
                Similar(diffs, $"{label} sub {i + 1} market", expected.SubSelections[i].Market, actual.SubSelections[i].Market);
                Similar(diffs, $"{label} sub {i + 1} selection", expected.SubSelections[i].Selection, actual.SubSelections[i].Selection);
            }
        }

        private static string? Compact(string? score) => score?.Replace(" ", "");

        private static void Exact<T>(List<string> diffs, string field, T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                diffs.Add($"{field}: expected {Show(expected)}, got {Show(actual)}{NotExtracted(expected, actual)}");
        }

        private static void ExactText(List<string> diffs, string field, string? expected, string? actual)
        {
            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                diffs.Add($"{field}: expected {Show(expected)}, got {Show(actual)}{NotExtracted(expected, actual)}");
        }

        private static void Similar(List<string> diffs, string field, string? expected, string? actual)
        {
            var ratio = TextSimilarity.Ratio(expected, actual);
            if (ratio < TextThreshold)
                diffs.Add($"{field}: expected {Show(expected)}, got {Show(actual)} (similarity {ratio:P0})");
        }

        private static string NotExtracted<T>(T expected, T actual) =>
            expected is not null && actual is null ? " (not extracted)" : "";

        private static string Show<T>(T value) => value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            _ => value.ToString() ?? "null"
        };
    }
}
