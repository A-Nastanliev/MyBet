using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public static class TestSlips
    {
        public static Slip ValidDouble(string slipId) => new()
        {
            SlipId = slipId,
            Type = "Двойни",
            Stake = 1.00m,
            TotalOdds = 2.92m,
            BonusPercent = 3,
            Payout = 2.98m,
            Status = SlipStatus.Won,
            Legs =
            {
                new Leg { HomeTeam = "Team A", AwayTeam = "Team B", Odds = 1.46m, Result = LegResult.Won },
                new Leg { HomeTeam = "Team C", AwayTeam = "Team D", Odds = 2.00m, Result = LegResult.Won }
            }
        };

        public static Slip WonWithEarlyPayout(string slipId)
        {
            var slip = ValidDouble(slipId);
            slip.Legs[1].Result = LegResult.EarlyPayout;
            slip.Legs[1].Score = "2 : 2";
            return slip;
        }

        public static Slip LostWithPendingLegs(string slipId) => new()
        {
            SlipId = slipId,
            Type = "Тройни",
            Stake = 1.00m,
            TotalOdds = 3.60m,
            Payout = 0.00m,
            Status = SlipStatus.Lost,
            Legs =
            {
                new Leg { HomeTeam = "Team A", AwayTeam = "Team B", Odds = 2.00m, Result = LegResult.Lost, Score = "1 : 0" },
                new Leg { HomeTeam = "Team C", AwayTeam = "Team D", Odds = 1.50m, Result = LegResult.Pending },
                new Leg { HomeTeam = "Team E", AwayTeam = "Team F", Odds = 1.20m, Result = LegResult.Pending }
            }
        };

        public static Slip CashedOutWithPendingLeg(string slipId) => new()
        {
            SlipId = slipId,
            Type = "Тройни",
            Stake = 3.00m,
            TotalOdds = 3.24m,
            Payout = 4.10m,
            Status = SlipStatus.CashedOut,
            Legs =
            {
                new Leg { HomeTeam = "Team A", AwayTeam = "Team B", Odds = 1.74m, Result = LegResult.Won },
                new Leg { HomeTeam = "Team C", AwayTeam = "Team D", Odds = 1.40m, Result = LegResult.EarlyPayout },
                new Leg { HomeTeam = "Team E", AwayTeam = "Team F", Odds = 1.33m, Result = LegResult.Pending }
            }
        };

        public static Slip WonLegAndPendingLeg(string slipId) => new()
        {
            SlipId = slipId,
            Type = "Двойни",
            Stake = 1.00m,
            TotalOdds = 2.92m,
            Status = null,
            Legs =
            {
                new Leg { HomeTeam = "Team A", AwayTeam = "Team B", Odds = 1.46m, Result = LegResult.Won },
                new Leg { HomeTeam = "Team C", AwayTeam = "Team D", Odds = 2.00m, Result = LegResult.Pending }
            }
        };

        public static Slip WithDroppedLeg(string slipId) => new()
        {
            SlipId = slipId,
            TotalOdds = 14.16m,
            Legs =
            {
                new Leg { Odds = 2.00m },
                new Leg { Odds = 2.00m }
            }
        };

        public static Slip DoubleWithAstonVillaOver(string slipId) => new()
        {
            SlipId = slipId,
            Type = "Двойни",
            Stake = 1.00m,
            TotalOdds = 2.92m,
            Legs =
            {
                new Leg
                {
                    HomeTeam = "Байер Леверкузен", AwayTeam = "1. ФК Нюрнберг",
                    Market = "Краен Резултат", Selection = "Байер Леверкузен",
                    Odds = 1.46m, Score = "4 : 2"
                },
                new Leg
                {
                    HomeTeam = "Борнемут", AwayTeam = "Астън Вила",
                    Market = "Астън Вила - Брой Голове 0.5", Selection = "Над 0.5",
                    Odds = 2.00m, Score = "3 : 0"
                }
            }
        };
    }
}