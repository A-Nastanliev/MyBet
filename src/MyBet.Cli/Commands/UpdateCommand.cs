using MyBet.Application.Import;
using MyBet.Application.Review;
using MyBet.Application.Update;

namespace MyBet.Cli.Commands
{
    public static class UpdateCommand
    {
        public static async Task<int> RunAsync(UpdateService update, ReviewService review, CancellationToken ct)
        {
            var result = await update.RunAsync(new ConsoleImportProgress(), ct);

            if (result.Status == UpdateStatus.NoHomeFolder)
            {
                Console.WriteLine("No home folder set. Use: mybet -f <path>");
                return 1;
            }

            if (result.Status == UpdateStatus.FolderMissing)
            {
                Console.WriteLine($"Home folder not found: {result.Folder}");
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine(
                $"Imported: {result.CountOf(ImportOutcome.Imported)}, " +
                $"needs review: {result.CountOf(ImportOutcome.NeedsReview)}, " +
                $"duplicates: {result.CountOf(ImportOutcome.Duplicate)}, " +
                $"already scanned: {result.CountOf(ImportOutcome.AlreadyScanned)}, " +
                $"failed: {result.CountOf(ImportOutcome.Failed)}");

            if (result.ReviewQueueCount > 0)
            {
                Console.Write($"{result.ReviewQueueCount} slips need review. Review now? (y/n) ");

                if (Console.ReadLine()?.Trim().StartsWith('y') == true)
                    return await ReviewCommand.RunAsync(review, ct);

                Console.WriteLine("Run mybet -x to review later.");
            }

            return 0;
        }
    }
}