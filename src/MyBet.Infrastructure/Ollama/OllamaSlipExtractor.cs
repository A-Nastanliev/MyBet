using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Infrastructure.Ollama.Dtos;
using MyBet.Infrastructure.Ollama.Mapping;

namespace MyBet.Infrastructure.Ollama
{
    public class OllamaSlipExtractor : ISlipExtractor
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private readonly HttpClient _http;
        private readonly OllamaOptions _options;

        public OllamaSlipExtractor(HttpClient http, IOptions<OllamaOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<Slip?> ExtractAsync(byte[] imageBytes, CancellationToken ct = default)
        {
            var request = new
            {
                model = _options.Model,
                stream = false,
                format = OllamaSlipSchema.Value,
                options = new { num_ctx = _options.NumCtx, temperature = 0 },
                messages = new[]
                {
                new
                {
                    role = "user",
                    content = OllamaPrompt.Text,
                    images = new[] { Convert.ToBase64String(imageBytes) }
                }
            }
            };

            using var response = await _http.PostAsJsonAsync("/api/chat", request, ct);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, ct);
            var content = envelope?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content)) return null;

            var dto = JsonSerializer.Deserialize<SlipDto>(content, JsonOptions);
            return dto is null ? null : SlipMapper.ToDomain(dto);
        }
    }
}
