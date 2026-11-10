using System.Globalization;
using MyBet.Application.Stats;
using MyBet.Domain.Enums;

namespace MyBet.Cli.Commands
{
    public static class StatsCommand
    {
        public static async Task<int> RunAsync(
            StatsService stats, StatsMetric? metric, IReadOnlyCollection<SlipStatus> statuses, CancellationToken ct)
        {
            var result = await stats.GetAsync(statuses, ct);

            if (metric is not null)
                PrintMetric(metric.Value, result);
            else if (statuses.Count > 0)
                PrintMiniSummary(result);
            else
                PrintFullSummary(result);

            return 0;
        }

        private static void PrintMetric(StatsMetric metric, StatsResult stats)
        {
            switch (metric)
            {
                case StatsMetric.Stakes: Console.WriteLine($"Staked: {Money(stats.Staked)}"); break;
                case StatsMetric.Returns: Console.WriteLine($"Returned: {Money(stats.Returned)}"); break;
                case StatsMetric.Profit: Console.WriteLine($"Profit: {Money(stats.Profit)}"); break;
            }
        }

        private static void PrintMiniSummary(StatsResult stats)
        {
            Console.WriteLine($"Slips: {stats.Count}");
            Console.WriteLine($"Staked: {Money(stats.Staked)}");
            Console.WriteLine($"Returned: {Money(stats.Returned)}");
            Console.WriteLine($"Profit: {Money(stats.Profit)}");
        }

        private static void PrintFullSummary(StatsResult stats)
        {
            Console.WriteLine($"Total slips: {stats.Count}");
            Console.WriteLine($"  Won: {stats.WonCount} ({Percent(stats.WonCount, stats.Count)})");
            Console.WriteLine($"  Cashed out: {stats.CashedOutCount} ({Percent(stats.CashedOutCount, stats.Count)})");
            Console.WriteLine($"  Lost: {stats.LostCount} ({Percent(stats.LostCount, stats.Count)})");
            Console.WriteLine($"Staked:   {Money(stats.Staked)}");
            Console.WriteLine($"Returned: {Money(stats.Returned)}");
            Console.WriteLine($"Profit:   {Money(stats.Profit)}");
            Console.WriteLine($"ROI:      {(stats.Roi is null ? "n/a" : (stats.Roi.Value * 100).ToString("F1", CultureInfo.InvariantCulture) + "%")}");
            Console.WriteLine($"Needs review: {stats.ReviewQueueCount}");
        }

        private static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

        private static string Percent(int part, int total) =>
            total == 0 ? "0.0%" : (part * 100m / total).ToString("F1", CultureInfo.InvariantCulture) + "%";
    }
}