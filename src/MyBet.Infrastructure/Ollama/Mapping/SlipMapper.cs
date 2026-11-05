using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;
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
        private static readonly Regex DateRegex = new(@"\b\d{1,2}\.\d{1,2}\.\d{4}\b");
        private static readonly Regex TimeRegex = new(@"\b\d{1,2}:\d{2}\b");
        private static readonly Regex ScoreRegex = new(@"^\s*(\d+)\s*[:\-–]\s*(\d+)\s*$");

        public static Slip ToDomain(SlipDto dto)
        {
            var slip = new Slip
            {
                SlipId = dto.SlipId?.Trim() ?? "",
                PlacedAt = ParseDate(dto.PlacedAt),
                Stake = dto.Stake,
                TotalOdds = dto.TotalOdds ?? 0m,
                Payout = dto.Payout,
                BonusPercent = dto.BonusPercent,
                BonusAmount = dto.BonusAmount,
                Legs = (dto.Legs ?? new()).Select(ToDomain).ToList()
            };

            slip.Type = string.IsNullOrWhiteSpace(dto.Type) ? DeriveType(slip.Legs.Count) : dto.Type.Trim();

            var cashedOut = dto.PayoutLabel?.Contains("Кешаут", StringComparison.OrdinalIgnoreCase) == true;
            slip.Status = cashedOut ? SlipStatus.CashedOut : LegResultEvaluator.ResolveStatus(slip.Legs);
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
                Result = DeriveResult(dto.Badges),
                Score = CleanScore(dto.Score),
                SubSelections = subs,
                IsBetBuilder = isBetBuilder
            };
        }

        private static string? DeriveType(int legCount) => legCount switch
        {
            < 1 => null,
            1 => "Сингъл",
            2 => "Двойни",
            3 => "Тройни",
            _ => $"{legCount}-кратни"
        };

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

        private static LegResult DeriveResult(IEnumerable<string>? badges)
        {
            var text = string.Join(" ", badges ?? Array.Empty<string>());
            if (Has(text, "Изплатени")) return LegResult.EarlyPayout;
            if (Has(text, "Загуба")) return LegResult.Lost;
            if (Has(text, "Печалба")) return LegResult.Won;
            return LegResult.Pending;
        }

        private static bool Has(string text, string word) =>
            text.Contains(word, StringComparison.OrdinalIgnoreCase);

        private static string? CleanScore(string? text)
        {
            var m = ScoreRegex.Match(text ?? "");
            return m.Success ? $"{m.Groups[1].Value} : {m.Groups[2].Value}" : null;
        }
    }
}