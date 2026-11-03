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
                new Leg { HomeTeam = "Team A", AwayTeam = "Team B", Odds = 1.46m },
                new Leg { HomeTeam = "Team C", AwayTeam = "Team D", Odds = 2.00m }
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
