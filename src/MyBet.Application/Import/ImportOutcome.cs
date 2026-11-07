namespace MyBet.Application.Import
{
    public enum ImportOutcome
    {
        Imported,
        NeedsReview,
        Duplicate,
        AlreadyScanned,
        Failed
    }
}
