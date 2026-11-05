using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;

namespace MyBet.Tests.Domain
{
    public class SlipValidatorTests
    {
        private static Leg Won(decimal odds) => new() { Odds = odds, Result = LegResult.Won };

        [Fact]
        public void Double_with_3_percent_bonus_is_valid()
        {
            var slip = new Slip
            {
                Stake = 1.00m,
                TotalOdds = 2.92m,
                BonusPercent = 3,
                Payout = 2.98m,
                Status = SlipStatus.Won,
                Legs = { Won(1.46m), Won(2.00m) }
            };

            Assert.Empty(SlipValidator.Validate(slip));
        }

        [Fact]
        public void Wrong_payout_is_flagged()
        {
            var slip = new Slip
            {
                Stake = 1.00m,
                TotalOdds = 2.92m,
                BonusPercent = 3,
                Payout = 5.00m,
                Status = SlipStatus.Won,
                Legs = { Won(1.46m), Won(2.00m) }
            };

            Assert.NotEmpty(SlipValidator.Validate(slip));
        }

        [Fact]
        public void Dropped_leg_is_flagged()
        {
            var slip = new Slip
            {
                TotalOdds = 14.16m,
                Legs = { new Leg { Odds = 2.00m }, new Leg { Odds = 2.00m } }
            };

            Assert.NotEmpty(SlipValidator.Validate(slip));
        }

        [Fact]
        public void Nine_leg_accumulator_with_10_percent_bonus_is_valid()
        {
            var slip = new Slip
            {
                Type = "9-кратни",
                Stake = 2.31m,
                TotalOdds = 14.16m,
                BonusPercent = 10,
                Payout = 35.75m,
                Status = SlipStatus.Won,
                Legs =
                {
                    Won(1.45m),
                    Won(1.5m),
                    Won(1.28m),
                    Won(1.19m),
                    Won(1.29m),
                    Won(1.49m),
                    Won(1.19m),
                    Won(1.46m),
                    Won(1.28m)
                }
            };

            Assert.Empty(SlipValidator.Validate(slip));
        }

        [Fact]
        public void Won_slip_with_a_lost_leg_is_flagged()
        {
            var slip = new Slip
            {
                TotalOdds = 2.92m,
                Status = SlipStatus.Won,
                Legs =
                {
                    Won(1.46m),
                    new Leg { Odds = 2.00m, Result = LegResult.Lost }
                }
            };

            Assert.Contains(SlipValidator.Validate(slip), i => i.Contains("Won but a leg is Lost"));
        }

        [Fact]
        public void Lost_slip_with_a_lost_leg_is_valid()
        {
            var slip = new Slip
            {
                Stake = 1.00m,
                TotalOdds = 2.92m,
                Status = SlipStatus.Lost,
                Legs =
                {
                    Won(1.46m),
                    new Leg { Odds = 2.00m, Result = LegResult.Lost }
                }
            };

            Assert.Empty(SlipValidator.Validate(slip));
        }

        [Fact]
        public void Cashed_out_slip_with_pending_legs_ignores_payout_math()
        {
            var slip = new Slip
            {
                Stake = 3.00m,
                TotalOdds = 6.05m,
                Payout = 6.58m,
                Status = SlipStatus.CashedOut,
                Legs =
                {
                    new Leg { Odds = 1.74m, Result = LegResult.Pending },
                    new Leg { Odds = 1.40m, Result = LegResult.EarlyPayout },
                    new Leg { Odds = 1.33m, Result = LegResult.Pending },
                    new Leg { Odds = 1.46m, Result = LegResult.EarlyPayout },
                    new Leg { Odds = 1.28m, Result = LegResult.Won }
                }
            };

            Assert.Empty(SlipValidator.Validate(slip));
        }
    }
}