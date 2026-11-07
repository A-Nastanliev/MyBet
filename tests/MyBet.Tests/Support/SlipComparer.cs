using MyBet.Domain.Entities;

namespace MyBet.Tests.Support
{
    public static class SlipComparer
    {
        public static IReadOnlyList<string> Compare(ExpectedSlip expected, Slip actual)
        {
            var diffs = new List<string>();

            ExactText(diffs, "slipId", expected.SlipId, actual.SlipId);
            Exact(diffs, "stake", expected.Stake, actual.Stake);
            Exact(diffs, "totalOdds", expected.TotalOdds, actual.TotalOdds);
            Exact(diffs, "payout", expected.Payout, actual.Payout);
            Exact(diffs, "bonusPercent", expected.BonusPercent, actual.BonusPercent);

            return diffs;
        }

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