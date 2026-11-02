using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyBet.Application.Abstractions;
using MyBet.Infrastructure.Ollama;

namespace MyBet.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));

            static void ConfigureClient(IServiceProvider sp, HttpClient client)
            {
                var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(options.TimeoutMinutes);
            }

            services.AddHttpClient<ISlipExtractor, OllamaSlipExtractor>(ConfigureClient);
            services.AddHttpClient<OllamaLifecycle>(ConfigureClient);

            return services;
        }
    }
}
