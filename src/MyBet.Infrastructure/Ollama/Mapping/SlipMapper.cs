using System.Text.RegularExpressions;
using MyBet.Domain.Entities;
using MyBet.Domain.Validation;
using MyBet.Infrastructure.Ollama.Dtos;

namespace MyBet.Infrastructure.Ollama.Mapping
{
    public static class SlipMapper
    {
        private static readonly Regex IdLabel =
            new(@"^[\s№#:]*(?:ref\.?[\s#:]*)?", RegexOptions.IgnoreCase);

        public static Slip ToDomain(SlipDto dto)
        {
            var slip = new Slip
            {
                SlipId = CleanSlipId(dto.SlipId),
                Stake = dto.Stake,
                TotalOdds = dto.TotalOdds,
                Payout = dto.Payout,
                BonusPercent = dto.BonusPercent is 0m ? null : dto.BonusPercent
            };

            slip.Status = SlipStatusResolver.Resolve(slip);
            slip.NeedsReview = slip.Status is null || slip.SlipId.Length == 0;
            return slip;
        }

        private static string CleanSlipId(string? raw) =>
            IdLabel.Replace((raw ?? "").Trim(), "").Trim();
    }
}