using MyBet.Application.Import;
using MyBet.Application.Update;

namespace MyBet.Cli.Commands
{
    public static class UpdateCommand
    {
        public static async Task<int> RunAsync(UpdateService update, CancellationToken ct)
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
                Console.WriteLine($"{result.ReviewQueueCount} slips need review. Run: mybet -x");

            return 0;
        }
    }
}