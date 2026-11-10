using MyBet.Application.Import;

namespace MyBet.Application.Update
{
    public record UpdateResult(
        UpdateStatus Status,
        string? Folder,
        IReadOnlyList<ImportResult> Imports,
        int ReviewQueueCount)
    {
        public int CountOf(ImportOutcome outcome) => Imports.Count(i => i.Outcome == outcome);
    }
}