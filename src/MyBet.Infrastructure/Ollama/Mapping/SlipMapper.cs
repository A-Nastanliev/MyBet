using MyBet.Domain.Entities;
using MyBet.Domain.Validation;
using MyBet.Infrastructure.Ollama.Dtos;

namespace MyBet.Infrastructure.Ollama.Mapping
{
    public static class SlipMapper
    {
        public static Slip ToDomain(SlipDto dto)
        {
            var slip = new Slip
            {
                SlipId = new string((dto.SlipId ?? "").Where(char.IsDigit).ToArray()),
                Stake = dto.Stake,
                TotalOdds = dto.TotalOdds,
                Payout = dto.Payout,
                BonusPercent = dto.BonusPercent is 0m ? null : dto.BonusPercent
            };

            slip.Status = SlipStatusResolver.Resolve(slip);
            slip.NeedsReview = slip.Status is null || slip.SlipId.Length == 0;
            return slip;
        }
    }
}