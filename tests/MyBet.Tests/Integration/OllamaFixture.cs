using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyBet.Application.Abstractions;
using MyBet.Infrastructure.Ollama;
using MyBet.Infrastructure;

namespace MyBet.Tests.Integration
{
    public sealed class OllamaFixture : IAsyncLifetime
    {
        private readonly ServiceProvider _provider;

        public string Model { get; }
        public ISlipExtractor Extractor { get; }

        public OllamaFixture()
        {
            var model = Environment.GetEnvironmentVariable("MYBET_OLLAMA_MODEL");

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddInfrastructure(o =>
            {
                if (!string.IsNullOrWhiteSpace(model)) o.Model = model;
            });

            _provider = services.BuildServiceProvider();
            Model = _provider.GetRequiredService<IOptions<OllamaOptions>>().Value.Model;
            Extractor = _provider.GetRequiredService<ISlipExtractor>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _provider.DisposeAsync();
    }
}