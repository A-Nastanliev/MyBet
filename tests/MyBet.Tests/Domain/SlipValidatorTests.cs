using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;

namespace MyBet.Tests.Domain
{
    public class SlipValidatorTests
    {
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
                Legs = { new Leg { Odds = 1.46m }, new Leg { Odds = 2.00m } }
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
                Legs = { new Leg { Odds = 1.46m }, new Leg { Odds = 2.00m } }
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
                    new Leg { Odds = 1.45m },
                    new Leg { Odds = 1.5m },
                    new Leg { Odds = 1.28m }, 
                    new Leg { Odds = 1.19m },
                    new Leg { Odds = 1.29m }, 
                    new Leg { Odds = 1.49m }, 
                    new Leg { Odds = 1.19m }, 
                    new Leg { Odds = 1.46m }, 
                    new Leg { Odds = 1.28m }  
                }
            };

            Assert.Empty(SlipValidator.Validate(slip));
        }
    }
}
