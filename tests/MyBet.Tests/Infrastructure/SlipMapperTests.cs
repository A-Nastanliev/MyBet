using MyBet.Domain.Enums;
using MyBet.Infrastructure.Ollama.Dtos;
using MyBet.Infrastructure.Ollama.Mapping;

namespace MyBet.Tests.Infrastructure
{
    public class SlipMapperTests
    {
        private static SlipDto Dto(string? slipId = "1001", decimal? stake = 1m, decimal? odds = 2.58m,
            decimal? payout = 2.63m, decimal? bonus = 3m) =>
            new() { SlipId = slipId, Stake = stake, TotalOdds = odds, Payout = payout, BonusPercent = bonus };

        [Fact]
        public void Slip_number_keeps_only_digits() =>
            Assert.Equal("15636169921438697040", SlipMapper.ToDomain(Dto("№15636169921438697040")).SlipId);

        [Fact]
        public void Zero_bonus_becomes_null() =>
            Assert.Null(SlipMapper.ToDomain(Dto(bonus: 0m)).BonusPercent);

        [Fact]
        public void Won_slip_is_not_flagged()
        {
            var slip = SlipMapper.ToDomain(Dto());
            Assert.Equal(SlipStatus.Won, slip.Status);
            Assert.False(slip.NeedsReview);
        }

        [Fact]
        public void Missing_payout_is_Lost() =>
            Assert.Equal(SlipStatus.Lost, SlipMapper.ToDomain(Dto(payout: null)).Status);

        [Fact]
        public void Undetermined_status_needs_review()
        {
            var slip = SlipMapper.ToDomain(Dto(odds: null));
            Assert.Null(slip.Status);
            Assert.True(slip.NeedsReview);
        }

        [Fact]
        public void Missing_slip_number_needs_review() =>
            Assert.True(SlipMapper.ToDomain(Dto(slipId: null)).NeedsReview);
    }
}