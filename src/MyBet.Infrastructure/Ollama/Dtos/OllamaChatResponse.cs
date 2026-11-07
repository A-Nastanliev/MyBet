using System.Text.Json.Serialization;

namespace MyBet.Infrastructure.Ollama.Dtos
{
    public class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }
        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }
    }
}
