using MyBet.Application.HomeFolder;

namespace MyBet.Cli.Commands
{
    public static class HomeFolderCommand
    {
        public static async Task<int> RunAsync(
            HomeFolderService folders, string? path, bool skipConfirmation, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var current = await folders.GetAsync(ct);
                Console.WriteLine(current is null
                    ? "No home folder set. Use: mybet -f <path>"
                    : $"Home folder: {current}");
                return 0;
            }

            var result = await folders.SetAsync(path, skipConfirmation, ct);

            if (result.Outcome == HomeFolderOutcome.ConfirmationRequired)
            {
                if (!Ask("Changing the home folder wipes the database. Continue?"))
                {
                    Console.WriteLine("Cancelled. The home folder was not changed.");
                    return 0;
                }

                result = await folders.SetAsync(path, wipeConfirmed: true, ct);
            }

            Console.WriteLine(result.Outcome switch
            {
                HomeFolderOutcome.Set => $"Home folder set to {result.Folder}",
                HomeFolderOutcome.Wiped => $"Home folder set to {result.Folder}. The database was wiped.",
                HomeFolderOutcome.Unchanged => $"Home folder is already {result.Folder}. Nothing changed.",
                HomeFolderOutcome.NotFound => $"Folder not found: {result.Folder}",
                _ => throw new ArgumentOutOfRangeException()
            });

            return result.Outcome == HomeFolderOutcome.NotFound ? 1 : 0;
        }

        private static bool Ask(string question)
        {
            Console.Write($"{question} (y/n) ");
            var answer = Console.ReadLine()?.Trim();
            return answer is not null &&
                   (answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    answer.Equals("yes", StringComparison.OrdinalIgnoreCase));
        }
    }
}