using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyBet.Application.Abstractions;
using MyBet.Infrastructure.Ollama;

namespace MyBet.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        private const string LifecycleClientName = "ollama-lifecycle";

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
            return services.AddOllama();
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<OllamaOptions> configure)
        {
            services.Configure(configure);
            return services.AddOllama();
        }

        private static IServiceCollection AddOllama(this IServiceCollection services)
        {
            static void ConfigureClient(IServiceProvider sp, HttpClient client)
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(options.TimeoutMinutes);
            }

            services.AddHttpClient(LifecycleClientName, ConfigureClient);
            services.AddSingleton(sp => new OllamaLifecycle(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(LifecycleClientName),
                sp.GetRequiredService<IOptions<OllamaOptions>>(),
                sp.GetRequiredService<ILogger<OllamaLifecycle>>()));

            services.AddHttpClient<ISlipExtractor, OllamaSlipExtractor>(ConfigureClient);

            return services;
        }
    }
}
