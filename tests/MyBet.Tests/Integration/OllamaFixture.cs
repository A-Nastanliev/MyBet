using Microsoft.Extensions.Options;
using MyBet.Application.Abstractions;
using MyBet.Infrastructure.Ollama;

namespace MyBet.Tests.Integration
{
    public sealed class OllamaFixture : IAsyncLifetime
    {
        private readonly HttpClient _http;
        private readonly OllamaLifecycle _lifecycle;
        private readonly OllamaSlipExtractor _extractor;
        private readonly Lazy<Task> _start;

        public string Model { get; }

        public OllamaFixture()
        {
            var settings = new OllamaOptions();

            var model = Environment.GetEnvironmentVariable("MYBET_OLLAMA_MODEL");
            if (!string.IsNullOrWhiteSpace(model)) settings.Model = model;

            Model = settings.Model;

            _http = new HttpClient
            {
                BaseAddress = new Uri(settings.BaseUrl),
                Timeout = TimeSpan.FromMinutes(settings.TimeoutMinutes)
            };

            var options = Options.Create(settings);
            _lifecycle = new OllamaLifecycle(_http, options);
            _extractor = new OllamaSlipExtractor(_http, options);
            _start = new Lazy<Task>(() => _lifecycle.StartAsync());
        }

        public async Task<ISlipExtractor> GetExtractorAsync()
        {
            await _start.Value;
            return _extractor;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _lifecycle.DisposeAsync();
            _http.Dispose();
        }
    }
}