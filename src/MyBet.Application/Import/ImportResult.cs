namespace MyBet.Application.Import
{
    public record ImportResult(
      string FilePath,
      ImportOutcome Outcome,
      string? SlipId,
      IReadOnlyList<string> Issues);
}
