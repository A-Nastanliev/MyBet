using MyBet.Domain.Entities;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Mapping
{
    internal static class SlipMapping
    {
        public static SlipRecord ToRecord(this Slip slip) => new()
        {
            SlipId = slip.SlipId,
            Stake = slip.Stake,
            TotalOdds = slip.TotalOdds,
            Payout = slip.Payout,
            BonusPercent = slip.BonusPercent,
            Status = slip.Status
        };

        public static Slip ToDomain(this SlipRecord record) => new()
        {
            Id = record.Id,
            SlipId = record.SlipId,
            Stake = record.Stake,
            TotalOdds = record.TotalOdds,
            Payout = record.Payout,
            BonusPercent = record.BonusPercent,
            Status = record.Status
        };
    }
}
