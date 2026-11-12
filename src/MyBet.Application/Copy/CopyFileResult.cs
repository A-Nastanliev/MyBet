namespace MyBet.Application.Copy
{
    public record CopyFileResult(string Source, CopyOutcome Outcome, string? Destination, string? Error);
}