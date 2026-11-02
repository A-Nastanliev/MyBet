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
    }
}
