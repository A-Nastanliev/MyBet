using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public class SlipComparerTests
    {
        private static ExpectedSlip Expected() => new()
        {
            SlipId = "1001",
            Type = "Двойни",
            Stake = 1.00m,
            TotalOdds = 2.92m,
            Payout = 2.98m,
            BonusPercent = 3,
            BonusAmount = 0.06m,
            Status = SlipStatus.Won,
            Legs =
            {
                new ExpectedLeg
                {
                    HomeTeam = "Бодьо", AwayTeam = "Глимт", Market = "Краен Резултат",
                    Selection = "1", Odds = 1.46m, Score = "4 : 3"
                },
                new ExpectedLeg
                {
                    HomeTeam = "Ювентус", AwayTeam = "Милан", Market = "Създай Залог",
                    Selection = "Създай Залог", Odds = 2.00m, IsBetBuilder = true,
                    SubSelections =
                    {
                        new ExpectedSubSelection { Market = "Голове", Selection = "Над 2.5" },
                        new ExpectedSubSelection { Market = "Двата отбора вкарват", Selection = "Да" }
                    }
                }
            }
        };

        private static Slip Actual() => new()
        {
            SlipId = "1001",
            Type = "Двойни",
            Stake = 1.00m,
            TotalOdds = 2.92m,
            Payout = 2.98m,
            BonusPercent = 3,
            BonusAmount = 0.06m,
            Legs =
            {
                new Leg
                {
                    HomeTeam = "Бодьо", AwayTeam = "Глимт", Market = "Краен Резултат",
                    Selection = "1", Odds = 1.46m, Score = "4 : 3"
                },
                new Leg
                {
                    HomeTeam = "Ювентус", AwayTeam = "Милан", Market = "Създай Залог",
                    Selection = "Създай Залог", Odds = 2.00m, IsBetBuilder = true,
                    SubSelections =
                    {
                        new SubSelection { Market = "Голове", Selection = "Над 2.5" },
                        new SubSelection { Market = "Двата отбора вкарват", Selection = "Да" }
                    }
                }
            }
        };

        [Fact]
        public void Matching_slip_has_no_differences()
        {
            Assert.Empty(SlipComparer.Compare(Expected(), Actual()));
        }

        [Fact]
        public void Wrong_leg_odds_are_reported()
        {
            var actual = Actual();
            actual.Legs[0].Odds = 1.48m;

            Assert.Contains(SlipComparer.Compare(Expected(), actual), d => d.Contains("leg 1 odds"));
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
            expected.Stake = null;

            Assert.Contains(SlipComparer.Compare(expected, Actual()), d => d.StartsWith("stake"));
        }

        [Fact]
        public void Misread_letter_in_a_team_name_is_tolerated()
        {
            var expected = Expected();
            expected.Legs[0].HomeTeam = "Бодьо/Глимт";
            var actual = Actual();
            actual.Legs[0].HomeTeam = "Бодо/Глинт";

            Assert.Empty(SlipComparer.Compare(expected, actual));
        }

        [Fact]
        public void Unrelated_team_name_is_reported()
        {
            var actual = Actual();
            actual.Legs[0].HomeTeam = "Реал Мадрид";

            Assert.Contains(SlipComparer.Compare(Expected(), actual), d => d.Contains("leg 1 homeTeam"));
        }

        [Fact]
        public void Spacing_inside_the_score_is_ignored()
        {
            var actual = Actual();
            actual.Legs[0].Score = "4:3";

            Assert.Empty(SlipComparer.Compare(Expected(), actual));
        }

        [Fact]
        public void Dropped_leg_reports_the_count_and_skips_the_leg_comparison()
        {
            var actual = Actual();
            actual.Legs.RemoveAt(1);

            var diff = Assert.Single(SlipComparer.Compare(Expected(), actual));
            Assert.StartsWith("legs: expected 2, got 1", diff);
        }

        [Fact]
        public void Missing_sub_selection_is_reported()
        {
            var actual = Actual();
            actual.Legs[1].SubSelections.RemoveAt(1);

            Assert.Contains(SlipComparer.Compare(Expected(), actual), d => d.Contains("leg 2 subSelections"));
        }

        [Fact]
        public void All_differences_are_reported_together()
        {
            var actual = Actual();
            actual.Stake = 5m;
            actual.Payout = 9m;
            actual.Legs[0].Odds = 1.1m;

            Assert.Equal(3, SlipComparer.Compare(Expected(), actual).Count);
        }
    }
}