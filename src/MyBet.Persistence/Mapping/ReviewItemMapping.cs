using MyBet.Domain.Entities;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Mapping
{
    internal static class ReviewItemMapping
    {
        public static ReviewItemRecord ToRecord(this ReviewItem item) => new()
        {
            FileHash = item.FileHash,
            FilePath = item.FilePath,
            SlipId = item.Extracted.SlipId,
            Stake = item.Extracted.Stake,
            TotalOdds = item.Extracted.TotalOdds,
            Payout = item.Extracted.Payout,
            BonusPercent = item.Extracted.BonusPercent
        };

        public static ReviewItem ToDomain(this ReviewItemRecord record) => new()
        {
            Id = record.Id,
            FileHash = record.FileHash,
            FilePath = record.FilePath,
            Extracted = new ExtractedSlip
            {
                SlipId = record.SlipId,
                Stake = record.Stake,
                TotalOdds = record.TotalOdds,
                Payout = record.Payout,
                BonusPercent = record.BonusPercent
            }
        };
    }
}
