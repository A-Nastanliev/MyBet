using MyBet.Application.Copy;

namespace MyBet.Cli.Commands
{
    public static class CopyCommand
    {
        public static async Task<int> RunAsync(CopyService copy, string path, CancellationToken ct)
        {
            var result = await copy.CopyAsync(path, ct);

            switch (result.Status)
            {
                case CopyStatus.NoHomeFolder:
                    Console.WriteLine("No home folder set. Use: mybet -f <path>");
                    return 1;

                case CopyStatus.HomeFolderMissing:
                    Console.WriteLine($"Home folder not found: {result.Location}");
                    return 1;

                case CopyStatus.SourceMissing:
                    Console.WriteLine($"Not found: {result.Location}");
                    return 1;

                case CopyStatus.NotAnImage:
                    Console.WriteLine($"Not a supported image (.png, .jpg, .jpeg, .webp): {result.Location}");
                    return 1;
            }

            if (result.Files.Count == 0)
            {
                Console.WriteLine("No images found.");
                return 0;
            }

            foreach (var file in result.Files)
                Console.WriteLine(Describe(file));

            Console.WriteLine();
            Console.WriteLine(
                $"Copied: {result.CountOf(CopyOutcome.Copied)}, " +
                $"renamed: {result.CountOf(CopyOutcome.Renamed)}, " +
                $"already there: {result.CountOf(CopyOutcome.SkippedSameContent)}, " +
                $"failed: {result.CountOf(CopyOutcome.Failed)}");

            if (result.CountOf(CopyOutcome.Copied) + result.CountOf(CopyOutcome.Renamed) > 0)
                Console.WriteLine("Run mybet -u to scan the new files.");

            return result.CountOf(CopyOutcome.Failed) > 0 ? 1 : 0;
        }

        private static string Describe(CopyFileResult file)
        {
            var name = Path.GetFileName(file.Source);

            return file.Outcome switch
            {
                CopyOutcome.Copied => $"copied:  {name}",
                CopyOutcome.Renamed => $"renamed: {name} -> {Path.GetFileName(file.Destination)}",
                CopyOutcome.SkippedSameContent => $"skipped: {name} (same content is already in the home folder)",
                _ => $"failed:  {name} ({file.Error})"
            };
        }
    }
}