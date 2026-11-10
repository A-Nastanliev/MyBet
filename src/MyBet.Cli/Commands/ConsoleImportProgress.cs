using MyBet.Application.Import;

namespace MyBet.Cli.Commands
{
    public class ConsoleImportProgress : IProgress<ImportProgress>
    {
        public void Report(ImportProgress progress)
        {
            var result = progress.Result;

            if (result.Outcome == ImportOutcome.AlreadyScanned) return;

            var id = result.SlipId is null ? "" : $" ({result.SlipId})";
            Console.WriteLine($"[{progress.Done}/{progress.Total}] {Path.GetFileName(result.FilePath)}: {result.Outcome}{id}");

            foreach (var issue in result.Issues)
                Console.WriteLine($"    {issue}");
        }
    }
}