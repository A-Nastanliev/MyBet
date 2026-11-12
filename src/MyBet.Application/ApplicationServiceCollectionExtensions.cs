using Microsoft.Extensions.DependencyInjection;
using MyBet.Application.Copy;
using MyBet.Application.HomeFolder;
using MyBet.Application.Import;
using MyBet.Application.Review;
using MyBet.Application.Stats;
using MyBet.Application.Update;

namespace MyBet.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ImportService>();
            services.AddScoped<HomeFolderService>();
            services.AddScoped<UpdateService>();
            services.AddScoped<StatsService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<CopyService>();
            return services;
        }
    }
}