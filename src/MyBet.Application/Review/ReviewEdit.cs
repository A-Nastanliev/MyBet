namespace MyBet.Application.Review
{
    public record ReviewEdit(
        string SlipId,
        decimal? Stake,
        decimal? TotalOdds,
        decimal? Payout,
        decimal? BonusPercent);
}