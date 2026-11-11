using System.Globalization;
using MyBet.Application.Review;
using MyBet.Domain.Entities;

namespace MyBet.Cli.Commands
{
    public static class ReviewCommand
    {
        public static async Task<int> RunAsync(ReviewService review, CancellationToken ct)
        {
            var queue = await review.GetQueueAsync(ct);
            if (queue.Count == 0)
            {
                Console.WriteLine("The review queue is empty.");
                return 0;
            }

            int saved = 0, discarded = 0, skipped = 0;

            for (var i = 0; i < queue.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                Console.WriteLine();
                Console.WriteLine($"[{i + 1}/{queue.Count}] {queue[i].FilePath}");
                Console.WriteLine("Enter keeps the value, - clears it.");

                switch (await ReviewItemAsync(review, queue[i], ct))
                {
                    case ReviewAction.Saved: saved++; break;
                    case ReviewAction.Discarded: discarded++; break;
                    default: skipped++; break;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Saved: {saved}, discarded: {discarded}, left in the queue: {skipped}");
            return 0;
        }

        private static async Task<ReviewAction> ReviewItemAsync(ReviewService review, ReviewItem item, CancellationToken ct)
        {
            var x = item.Extracted;
            var edit = new ReviewEdit(x.SlipId, x.Stake, x.TotalOdds, x.Payout, x.BonusPercent);

            while (true)
            {
                edit = Ask(edit);

                var preview = await review.PreviewAsync(edit, ct);
                Print(preview);

                var choice = preview.CanAccept
                    ? ReadChoice("Accept (y), edit again (e), skip (s), discard (d)? ", "yesd")
                    : ReadChoice("Edit again (e), skip (s), discard (d)? ", "esd");

                switch (choice)
                {
                    case 'e':
                        continue;

                    case 's':
                        return ReviewAction.Skipped;

                    case 'd':
                        await review.DiscardAsync(item.Id, ct);
                        Console.WriteLine("Discarded.");
                        return ReviewAction.Discarded;

                    default:
                        var result = await review.AcceptAsync(item.Id, edit, ct);
                        switch (result.Outcome)
                        {
                            case ReviewOutcome.Saved:
                                Console.WriteLine("Saved.");
                                return ReviewAction.Saved;

                            case ReviewOutcome.NotFound:
                                Console.WriteLine("This item is no longer in the queue.");
                                return ReviewAction.Skipped;

                            default:
                                Console.WriteLine("Could not save it, check the values.");
                                continue;
                        }
                }
            }
        }

        private static ReviewEdit Ask(ReviewEdit current) => new(
            AskText("slipId", current.SlipId),
            AskDecimal("stake", current.Stake),
            AskDecimal("totalOdds", current.TotalOdds),
            AskDecimal("payout", current.Payout),
            AskDecimal("bonusPercent", current.BonusPercent));

        private static string AskText(string label, string current)
        {
            Console.Write($"{label} [{current}]: ");
            var input = ReadLine();
            return input.Length == 0 ? current : input;
        }

        private static decimal? AskDecimal(string label, decimal? current)
        {
            while (true)
            {
                Console.Write($"{label} [{Format(current)}]: ");
                var input = ReadLine();

                if (input.Length == 0) return current;
                if (input == "-") return null;

                if (decimal.TryParse(input.Replace(',', '.'), NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var value))
                    return value;

                Console.WriteLine("Not a number. Use for example 2.50, or - for none.");
            }
        }

        private static void Print(ReviewPreview preview)
        {
            var s = preview.Slip;

            Console.WriteLine();
            Console.WriteLine($"  slipId:       {(s.SlipId.Length == 0 ? "none" : s.SlipId)}");
            Console.WriteLine($"  stake:        {Format(s.Stake)}");
            Console.WriteLine($"  totalOdds:    {Format(s.TotalOdds)}");
            Console.WriteLine($"  payout:       {Format(s.Payout)}");
            Console.WriteLine($"  bonusPercent: {Format(s.BonusPercent)}");
            Console.WriteLine($"  status:       {s.Status?.ToString() ?? "undetermined"}");

            if (preview.IsDuplicate)
                Console.WriteLine("  This slip reference is already saved.");
            else if (s.SlipId.Length == 0)
                Console.WriteLine("  The slip reference is empty.");
            else if (s.Status is null)
                Console.WriteLine("  The status cannot be determined from these numbers.");

            Console.WriteLine();
        }

        private static char ReadChoice(string prompt, string allowed)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = ReadLine();

                if (input.Length > 0 && allowed.Contains(char.ToLowerInvariant(input[0])))
                    return char.ToLowerInvariant(input[0]);
            }
        }

        private static string ReadLine() =>
            Console.ReadLine()?.Trim() ?? throw new OperationCanceledException();

        private static string Format(decimal? value) =>
            value?.ToString(CultureInfo.InvariantCulture) ?? "none";
    }
}