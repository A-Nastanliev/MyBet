using System.Text.Json;

namespace MyBet.Infrastructure.Ollama
{
    public static class OllamaSlipSchema
    {
        public static readonly JsonElement Value = JsonDocument.Parse("""
        {
          "type": "object",
          "properties": {
            "slipId":       { "type": ["string", "null"] },
            "stake":        { "type": ["number", "null"] },
            "totalOdds":    { "type": ["number", "null"] },
            "payout":       { "type": ["number", "null"] },
            "bonusPercent": { "type": ["number", "null"] }
          },
          "required": ["slipId", "stake", "totalOdds", "payout", "bonusPercent"]
        }
        """).RootElement.Clone();
    }
}