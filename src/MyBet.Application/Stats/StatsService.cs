using MyBet.Application.Abstractions;
using MyBet.Domain.Enums;

namespace MyBet.Application.Stats
{
    public class StatsService
    {
        private readonly ISlipRepository _slips;
        private readonly IReviewQueueRepository _reviewQueue;

        public StatsService(ISlipRepository slips, IReviewQueueRepository reviewQueue)
        {
            _slips = slips;
            _reviewQueue = reviewQueue;
        }

        public async Task<StatsResult> GetAsync(
            IReadOnlyCollection<SlipStatus> statuses, CancellationToken ct = default)
        {
            var all = await _slips.GetAllAsync(ct);

            var matching = statuses.Count == 0
                ? all
                : all.Where(s => statuses.Contains(s.Status)).ToList();

            return new StatsResult(
                Count: matching.Count,
                WonCount: matching.Count(s => s.Status == SlipStatus.Won),
                CashedOutCount: matching.Count(s => s.Status == SlipStatus.CashedOut),
                LostCount: matching.Count(s => s.Status == SlipStatus.Lost),
                Staked: matching.Sum(s => s.Stake),
                Returned: matching.Sum(s => s.Payout),
                ReviewQueueCount: await _reviewQueue.CountAsync(ct));
        }
    }
}