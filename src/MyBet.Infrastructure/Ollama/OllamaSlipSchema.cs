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
            "isLive":       { "type": ["boolean", "null"] },
            "placedAt":     { "type": ["string", "null"] },
            "type":         { "type": ["string", "null"] },
            "stake":        { "type": ["number", "null"] },
            "totalOdds":    { "type": ["number", "null"] },
            "payout":       { "type": ["number", "null"] },
            "bonusPercent": { "type": ["number", "null"] },
            "bonusAmount":  { "type": ["number", "null"] },
            "legs": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "selection": { "type": ["string", "null"] },
                  "odds":      { "type": ["number", "null"] },
                  "market":    { "type": ["string", "null"] },
                  "homeTeam":  { "type": ["string", "null"] },
                  "awayTeam":  { "type": ["string", "null"] },
                  "score":     { "type": ["string", "null"] },
                  "subSelections": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "selection": { "type": ["string", "null"] },
                        "market":    { "type": ["string", "null"] }
                      },
                      "required": ["selection", "market"]
                    }
                  }
                },
                "required": ["selection", "odds", "market", "homeTeam", "awayTeam", "score", "subSelections"]
              }
            }
          },
          "required": ["slipId", "isLive", "placedAt", "type", "stake", "totalOdds", "payout", "bonusPercent", "bonusAmount", "legs"]
        }
        """).RootElement.Clone();
    }
}