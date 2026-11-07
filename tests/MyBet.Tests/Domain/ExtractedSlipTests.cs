using MyBet.Domain.Enums;
using MyBet.Tests.Support;

namespace MyBet.Tests.Domain
{
    public class ExtractedSlipTests
    {
        [Fact]
        public void Complete_slip_does_not_need_review() =>
            Assert.False(TestSlips.ValidDouble("1001").NeedsReview);

        [Fact]
        public void Undetermined_status_needs_review() =>
            Assert.True(TestSlips.Undetermined("1001").NeedsReview);

        [Fact]
        public void Empty_slip_reference_needs_review() =>
            Assert.True(TestSlips.NoReference().NeedsReview);

        [Fact]
        public void Slip_is_created_from_a_complete_extraction()
        {
            var slip = TestSlips.ValidDouble("1001").ToSlip();

            Assert.Equal("1001", slip.SlipId);
            Assert.Equal(1m, slip.Stake);
            Assert.Equal(2.58m, slip.TotalOdds);
            Assert.Equal(2.63m, slip.Payout);
            Assert.Equal(3m, slip.BonusPercent);
            Assert.Equal(SlipStatus.Won, slip.Status);
        }

        [Fact]
        public void Missing_payout_is_saved_as_zero()
        {
            var slip = TestSlips.Lost("1002").ToSlip();

            Assert.Equal(0m, slip.Payout);
            Assert.Equal(SlipStatus.Lost, slip.Status);
        }

        [Fact]
        public void Slip_that_needs_review_cannot_be_saved() =>
            Assert.Throws<InvalidOperationException>(() => TestSlips.Undetermined("1003").ToSlip());
    }
}
