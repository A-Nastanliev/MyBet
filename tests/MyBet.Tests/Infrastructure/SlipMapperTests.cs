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

        [Theory]
        [InlineData("№15636169921438697040", "15636169921438697040")]
        [InlineData("№: 51425992", "51425992")]
        [InlineData("Ref.#BD480B5B-3547-4518-8CCE-2E1B1895B9F7", "BD480B5B-3547-4518-8CCE-2E1B1895B9F7")]
        [InlineData("bd480b5b-3547-4518-8cce-2e1b1895b9f7", "bd480b5b-3547-4518-8cce-2e1b1895b9f7")]
        [InlineData("Ref.#bd480b5b-3547-4518-8cce-2e1b1895b9f7", "bd480b5b-3547-4518-8cce-2e1b1895b9f7")]
        [InlineData("FbOAhzuOw148hoXv1eE8xQ==", "FbOAhzuOw148hoXv1eE8xQ==")]
        public void Slip_reference_is_kept_without_its_label(string raw, string expected) =>
            Assert.Equal(expected, SlipMapper.ToDomain(Dto(raw)).SlipId);

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