namespace MyBet.Application.Stats
{
    public record StatsResult(
        int Count, int WonCount, int CashedOutCount, int LostCount, decimal Staked, decimal Returned, int ReviewQueueCount)
    {
        public decimal Profit => Returned - Staked;

        public decimal? Roi => Staked == 0 ? null : Profit / Staked;
    }
}