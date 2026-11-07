using System.Text.Json.Serialization;

namespace MyBet.Infrastructure.Ollama.Dtos
{
    public class SlipDto
    {
        [JsonPropertyName("slipId")]
        public string? SlipId { get; set; }
        [JsonPropertyName("stake")] 
        public decimal? Stake { get; set; }
        [JsonPropertyName("totalOdds")] 
        public decimal? TotalOdds { get; set; }
        [JsonPropertyName("payout")] 
        public decimal? Payout { get; set; }
        [JsonPropertyName("bonusPercent")] 
        public decimal? BonusPercent { get; set; }
    }
}