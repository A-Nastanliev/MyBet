using MyBet.Application.Stats;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class StatsServiceTests
    {
        private static readonly SlipStatus[] All = Array.Empty<SlipStatus>();

        private readonly InMemorySlipRepository _slips = new();
        private readonly InMemoryReviewQueueRepository _reviewQueue = new();

        private StatsService Create() => new(_slips, _reviewQueue);

        private void AddSlip(string id, decimal stake, decimal payout, SlipStatus status) =>
            _slips.Add(new Slip { SlipId = id, Stake = stake, TotalOdds = 2m, Payout = payout, Status = status });

        private void AddTypicalSlips()
        {
            AddSlip("1", 10m, 25m, SlipStatus.Won);
            AddSlip("2", 10m, 12m, SlipStatus.CashedOut);
            AddSlip("3", 5m, 0m, SlipStatus.Lost);
        }

        [Fact]
        public async Task No_slips_gives_zeros_and_no_roi()
        {
            var result = await Create().GetAsync(All);

            Assert.Equal(0, result.Count);
            Assert.Equal(0m, result.Profit);
            Assert.Null(result.Roi);
        }

        [Fact]
        public async Task Totals_cover_all_slips_when_there_is_no_filter()
        {
            AddTypicalSlips();

            var result = await Create().GetAsync(All);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result.WonCount);
            Assert.Equal(1, result.CashedOutCount);
            Assert.Equal(1, result.LostCount);
            Assert.Equal(25m, result.Staked);
            Assert.Equal(37m, result.Returned);
            Assert.Equal(12m, result.Profit);
            Assert.Equal(0.48m, result.Roi);
        }

        [Fact]
        public async Task Several_statuses_mean_or()
        {
            AddTypicalSlips();

            var result = await Create().GetAsync(new[] { SlipStatus.Won, SlipStatus.CashedOut });

            Assert.Equal(2, result.Count);
            Assert.Equal(20m, result.Staked);
            Assert.Equal(37m, result.Returned);
        }

        [Fact]
        public async Task Lost_filter_gives_a_negative_profit()
        {
            AddTypicalSlips();

            var result = await Create().GetAsync(new[] { SlipStatus.Lost });

            Assert.Equal(1, result.Count);
            Assert.Equal(5m, result.Staked);
            Assert.Equal(-5m, result.Profit);
        }

        [Fact]
        public async Task Review_queue_is_counted_but_never_part_of_the_totals()
        {
            AddTypicalSlips();
            _reviewQueue.Add(new ReviewItem { FileHash = "h", FilePath = "p", Extracted = TestSlips.Undetermined("9") });

            var result = await Create().GetAsync(All);

            Assert.Equal(1, result.ReviewQueueCount);
            Assert.Equal(3, result.Count);
            Assert.Equal(25m, result.Staked);
        }
    }
}