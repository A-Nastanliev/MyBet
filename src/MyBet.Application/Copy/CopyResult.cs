namespace MyBet.Application.Copy
{
    public record CopyResult(CopyStatus Status, string Location, IReadOnlyList<CopyFileResult> Files)
    {
        public int CountOf(CopyOutcome outcome) => Files.Count(f => f.Outcome == outcome);
    }
}