using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;

namespace MyBet.Tests.Domain
{
    public class LegResultEvaluatorTests
    {
        private static Leg Goals(string market, string selection, string? score,
            string home = "Борнемут", string away = "Астън Вила") => new()
            {
                HomeTeam = home,
                AwayTeam = away,
                Market = market,
                Selection = selection,
                Score = score
            };

        private static Leg FullTime(string selection, string score) =>
            Goals("Краен Резултат", selection, score, "Байер Леверкузен", "1. ФК Нюрнберг");

        [Fact]
        public void Team_over_0_5_is_lost_when_that_team_scored_nothing()
        {
            var leg = Goals("Астън Вила - Брой Голове 0.5", "Над 0.5", "3 : 0");

            Assert.Equal(LegResult.Lost, LegResultEvaluator.Evaluate(leg));
        }

        [Fact]
        public void Team_over_0_5_is_won_when_that_team_scored()
        {
            var leg = Goals("Борнемут - Брой Голове 0.5", "Над 0.5", "3 : 0");

            Assert.Equal(LegResult.Won, LegResultEvaluator.Evaluate(leg));
        }

        [Fact]
        public void Unknown_team_prefix_is_not_guessed()
        {
            var leg = Goals("Реал - Брой Голове 0.5", "Над 0.5", "3 : 0");

            Assert.Null(LegResultEvaluator.Evaluate(leg));
        }

        [Fact]
        public void Total_goals_over_is_won_when_the_sum_is_above_the_line()
        {
            Assert.Equal(LegResult.Won,
                LegResultEvaluator.Evaluate(Goals("Брой Голове 2.5", "Над 2.5", "2 : 1")));
        }

        [Fact]
        public void Total_goals_over_is_lost_when_the_sum_is_below_the_line()
        {
            Assert.Equal(LegResult.Lost,
                LegResultEvaluator.Evaluate(Goals("Брой Голове 2.5", "Над 2.5", "1 : 1")));
        }

        [Fact]
        public void Total_goals_under_is_won_when_the_sum_is_below_the_line()
        {
            Assert.Equal(LegResult.Won,
                LegResultEvaluator.Evaluate(Goals("Брой Голове 3.5", "Под 3.5", "1 : 1")));
        }

        [Fact]
        public void Total_goals_under_is_lost_when_the_sum_is_above_the_line()
        {
            Assert.Equal(LegResult.Lost,
                LegResultEvaluator.Evaluate(Goals("Брой Голове 3.5", "Под 3.5", "4 : 3")));
        }

        [Fact]
        public void Score_without_spaces_is_understood()
        {
            Assert.Equal(LegResult.Won,
                LegResultEvaluator.Evaluate(Goals("Брой Голове 2.5", "Над 2.5", "2:1")));
        }

        [Fact]
        public void Whole_number_line_is_left_for_review()
        {
            Assert.Null(LegResultEvaluator.Evaluate(Goals("Брой Голове 2", "Над 2", "1 : 1")));
        }

        [Fact]
        public void Home_pick_is_won_when_home_leads()
        {
            Assert.Equal(LegResult.Won,
                LegResultEvaluator.Evaluate(FullTime("Байер Леверкузен", "4 : 2")));
        }

        [Fact]
        public void Home_pick_is_lost_when_away_leads()
        {
            Assert.Equal(LegResult.Lost,
                LegResultEvaluator.Evaluate(FullTime("Байер Леверкузен", "0 : 1")));
        }

        [Fact]
        public void Away_pick_is_won_when_away_leads()
        {
            Assert.Equal(LegResult.Won,
                LegResultEvaluator.Evaluate(FullTime("1. ФК Нюрнберг", "0 : 1")));
        }

        [Fact]
        public void Home_pick_is_lost_on_a_draw()
        {
            Assert.Equal(LegResult.Lost,
                LegResultEvaluator.Evaluate(FullTime("Байер Леверкузен", "1 : 1")));
        }

        [Fact]
        public void Pick_that_is_not_a_team_is_left_for_review()
        {
            Assert.Null(LegResultEvaluator.Evaluate(FullTime("Х", "1 : 1")));
        }

        [Fact]
        public void Unsupported_market_is_left_for_review()
        {
            Assert.Null(LegResultEvaluator.Evaluate(Goals("Двойна Възможност", "1X", "2 : 0")));
        }

        [Fact]
        public void Missing_score_is_left_for_review()
        {
            Assert.Null(LegResultEvaluator.Evaluate(Goals("Брой Голове 2.5", "Над 2.5", null)));
        }

        [Fact]
        public void Bet_builder_is_left_for_review()
        {
            var leg = Goals("Създай Залог", "", "2 : 1");
            leg.IsBetBuilder = true;

            Assert.Null(LegResultEvaluator.Evaluate(leg));
        }

        private static List<Leg> Legs(params LegResult?[] results) =>
            results.Select(r => new Leg { Result = r }).ToList();

        [Fact]
        public void Any_lost_leg_makes_the_slip_lost()
        {
            Assert.Equal(SlipStatus.Lost,
                LegResultEvaluator.ResolveStatus(Legs(LegResult.Won, LegResult.Lost, null)));
        }

        [Fact]
        public void All_won_legs_make_the_slip_won()
        {
            Assert.Equal(SlipStatus.Won,
                LegResultEvaluator.ResolveStatus(Legs(LegResult.Won, LegResult.Won)));
        }

        [Fact]
        public void An_undecided_leg_leaves_the_status_unknown()
        {
            Assert.Null(LegResultEvaluator.ResolveStatus(Legs(LegResult.Won, null)));
        }

        [Fact]
        public void Early_payout_leg_leaves_the_status_unknown()
        {
            Assert.Null(LegResultEvaluator.ResolveStatus(Legs(LegResult.Won, LegResult.EarlyPayout)));
        }

        [Fact]
        public void No_legs_leaves_the_status_unknown()
        {
            Assert.Null(LegResultEvaluator.ResolveStatus(new List<Leg>()));
        }
    }
}
