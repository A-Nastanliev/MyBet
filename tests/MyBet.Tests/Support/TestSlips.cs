using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public static class TestSlips
    {
        public static ExtractedSlip ValidDouble(string slipId) => new()
        {
            SlipId = slipId,
            Stake = 1m,
            TotalOdds = 2.58m,
            Payout = 2.63m,
            BonusPercent = 3m,
            Status = SlipStatus.Won
        };

        public static ExtractedSlip Lost(string slipId) => new()
        {
            SlipId = slipId,
            Stake = 2m,
            TotalOdds = 5m,
            Payout = null,
            Status = SlipStatus.Lost
        };

        public static ExtractedSlip CashedOut(string slipId) => new()
        {
            SlipId = slipId,
            Stake = 2m,
            TotalOdds = 5m,
            Payout = 4.10m,
            Status = SlipStatus.CashedOut
        };

        public static ExtractedSlip Undetermined(string slipId) => new()
        {
            SlipId = slipId,
            Stake = 2m,
            TotalOdds = null,
            Payout = 4.10m,
            Status = null
        };

        public static ExtractedSlip NoReference() => new()
        {
            SlipId = "",
            Stake = 1m,
            TotalOdds = 2.58m,
            Payout = 2.63m,
            BonusPercent = 3m,
            Status = SlipStatus.Won
        };

        public static Slip Saved(string slipId) => new()
        {
            SlipId = slipId,
            Stake = 1m,
            TotalOdds = 2.58m,
            Payout = 2.63m,
            BonusPercent = 3m,
            Status = SlipStatus.Won
        };
    }
}
