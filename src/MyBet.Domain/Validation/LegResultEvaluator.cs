using System.Text.RegularExpressions;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Domain.Validation
{
    public static class LegResultEvaluator
    {
        private const string GoalsMarket = "Брой Голове";
        private const string FullTimeResult = "Краен Резултат";

        private static readonly Regex ScoreRegex = new(@"(\d+)\s*:\s*(\d+)");
        private static readonly Regex LineRegex = new(@"^(Над|Под)\s+(\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase);

        public static LegResult? Evaluate(Leg leg)
        {
            if (leg.IsBetBuilder || leg.Result == LegResult.EarlyPayout) return null;

            var m = ScoreRegex.Match(leg.Score ?? "");
            if (!m.Success) return null;
            var home = int.Parse(m.Groups[1].Value);
            var away = int.Parse(m.Groups[2].Value);

            if (leg.Market.Contains(GoalsMarket, StringComparison.OrdinalIgnoreCase))
                return EvaluateGoals(leg, home, away);

            if (leg.Market.Equals(FullTimeResult, StringComparison.OrdinalIgnoreCase))
                return EvaluateFullTime(leg, home, away);

            return null;
        }

        private static LegResult? EvaluateGoals(Leg leg, int home, int away)
        {
            var m = LineRegex.Match(leg.Selection.Trim());
            if (!m.Success) return null;

            var line = decimal.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
            if (line % 1 != 0.5m) return null;

            var isOver = m.Groups[1].Value.Equals("Над", StringComparison.OrdinalIgnoreCase);

            int goals = home + away;
            var prefix = leg.Market[..leg.Market.IndexOf(GoalsMarket, StringComparison.OrdinalIgnoreCase)]
                .Replace("-", "").Trim();
            if (prefix.Length > 0)
            {
                if (Same(prefix, leg.HomeTeam)) goals = home;
                else if (Same(prefix, leg.AwayTeam)) goals = away;
                else return null;
            }

            return (isOver ? goals > line : goals < line) ? LegResult.Won : LegResult.Lost;
        }

        private static LegResult? EvaluateFullTime(Leg leg, int home, int away)
        {
            if (Same(leg.Selection, leg.HomeTeam)) return home > away ? LegResult.Won : LegResult.Lost;
            if (Same(leg.Selection, leg.AwayTeam)) return away > home ? LegResult.Won : LegResult.Lost;
            return null;
        }

        private static bool Same(string a, string b) =>
            string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

        public static SlipStatus? ResolveStatus(IReadOnlyCollection<Leg> legs)
        {
            if (legs.Count == 0) return null;
            if (legs.Any(l => l.Result == LegResult.Lost)) return SlipStatus.Lost;
            if (legs.All(l => l.Result is LegResult.Won or LegResult.EarlyPayout)) return SlipStatus.Won;
            return null;
        }
    }
}