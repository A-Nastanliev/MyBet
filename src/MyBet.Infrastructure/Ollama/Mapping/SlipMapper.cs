using MyBet.Domain.Entities;
using MyBet.Infrastructure.Ollama.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MyBet.Infrastructure.Ollama.Mapping
{
    public static class SlipMapper
    {
        private const string BetBuilderMarker = "Създай Залог";

        public static Slip ToDomain(SlipDto dto)
        {
            var isLive = dto.IsLive ?? false;

            var slip = new Slip
            {
                SlipId = dto.SlipId?.Trim() ?? "",
                IsLive = isLive,
                PlacedAt = isLive ? null : ParseDate(dto.PlacedAt),
                Stake = dto.Stake,
                TotalOdds = dto.TotalOdds ?? 0m,
                Payout = dto.Payout,
                BonusPercent = dto.BonusPercent,
                BonusAmount = dto.BonusAmount,
                Legs = (dto.Legs ?? new()).Select(ToDomain).ToList()
            };

            slip.Type = string.IsNullOrWhiteSpace(dto.Type) ? DeriveType(slip.Legs.Count) : dto.Type.Trim();
            return slip;
        }

        private static Leg ToDomain(LegDto dto)
        {
            var subs = (dto.SubSelections ?? new())
                .Select(s => new SubSelection { Market = s.Market ?? "", Selection = s.Selection ?? "" })
                .ToList();

            var market = dto.Market?.Trim() ?? "";
            var selection = dto.Selection?.Trim() ?? "";

            var isBetBuilder = subs.Count > 0
                || market.Contains(BetBuilderMarker, StringComparison.OrdinalIgnoreCase)
                || selection.Contains(BetBuilderMarker, StringComparison.OrdinalIgnoreCase);

            return new Leg
            {
                HomeTeam = dto.HomeTeam?.Trim() ?? "",
                AwayTeam = dto.AwayTeam?.Trim() ?? "",
                Market = isBetBuilder ? BetBuilderMarker : market,
                Selection = isBetBuilder ? "" : selection,
                Odds = dto.Odds ?? 0m,
                Score = dto.Score?.Trim(),
                SubSelections = subs,
                IsBetBuilder = isBetBuilder
            };
        }

        private static string? DeriveType(int legCount) => legCount switch
        {
            < 1 => null,
            1 => "Сингъл",
            2 => "Двойни",
            3=> "Тройни",
            _ => $"{legCount}-кратни"
        };

        private static readonly Regex DateRegex = new(@"\b\d{1,2}\.\d{1,2}\.\d{4}\b");
        private static readonly Regex TimeRegex = new(@"\b\d{1,2}:\d{2}\b");

        private static DateTime? ParseDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var dateMatch = DateRegex.Match(text);
            if (!dateMatch.Success
                || !DateTime.TryParseExact(dateMatch.Value, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return null;

            var timeMatch = TimeRegex.Match(text);
            if (timeMatch.Success && TimeSpan.TryParse(timeMatch.Value, CultureInfo.InvariantCulture, out var time))
                date = date.Add(time);

            return date;
        }
    }
}
