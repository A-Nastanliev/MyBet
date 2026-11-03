namespace MyBet.Infrastructure.Ollama.Dtos
{
    public class SlipDto
    {
        public string? SlipId { get; set; }
        public string? PlacedAt { get; set; }
        public string? Type { get; set; }
        public decimal? Stake { get; set; }
        public decimal? TotalOdds { get; set; }
        public decimal? Payout { get; set; }
        public decimal? BonusPercent { get; set; }
        public decimal? BonusAmount { get; set; }
        public List<LegDto>? Legs { get; set; }
    }
}
