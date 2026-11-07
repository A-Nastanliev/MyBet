using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public class SlipComparerTests
    {
        private static ExpectedSlip Expected() => new()
        {
            SlipId = "15636169921438697040",
            Stake = 1.00m,
            TotalOdds = 2.58m,
            Payout = 2.63m,
            BonusPercent = 3m,
            Status = SlipStatus.Won
        };

        private static Slip Actual() => new()
        {
            SlipId = "15636169921438697040",
            Stake = 1.00m,
            TotalOdds = 2.58m,
            Payout = 2.63m,
            BonusPercent = 3m,
            Status = SlipStatus.Won
        };

        [Fact]
        public void Matching_slip_has_no_differences() =>
            Assert.Empty(SlipComparer.Compare(Expected(), Actual()));

        [Fact]
        public void Scale_of_a_decimal_does_not_matter()
        {
            var actual = Actual();
            actual.Stake = 1m;
            actual.Payout = 2.630m;

            Assert.Empty(SlipComparer.Compare(Expected(), actual));
        }

        [Fact]
        public void Wrong_odds_are_reported()
        {
            var actual = Actual();
            actual.TotalOdds = 2.85m;

            Assert.Contains(SlipComparer.Compare(Expected(), actual), d => d.StartsWith("totalOdds"));
        }

        [Fact]
        public void Missing_value_is_marked_as_not_extracted()
        {
            var actual = Actual();
            actual.Stake = null;

            var diff = Assert.Single(SlipComparer.Compare(Expected(), actual));
            Assert.Contains("(not extracted)", diff);
        }

        [Fact]
        public void Expected_null_must_be_null()
        {
            var expected = Expected();
            expected.BonusPercent = null;

            Assert.Contains(SlipComparer.Compare(expected, Actual()), d => d.StartsWith("bonusPercent"));
        }

        [Fact]
        public void A_slip_number_with_a_wrong_digit_is_reported()
        {
            var actual = Actual();
            actual.SlipId = "15636169921438697041";

            Assert.Contains(SlipComparer.Compare(Expected(), actual), d => d.StartsWith("slipId"));
        }

        [Fact]
        public void All_differences_are_reported_together()
        {
            var actual = Actual();
            actual.Stake = 5m;
            actual.Payout = 9m;
            actual.TotalOdds = 1.1m;

            Assert.Equal(3, SlipComparer.Compare(Expected(), actual).Count);
        }
    }
}