using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBet.Application.Abstractions;
using MyBet.Persistence.Repositories;

namespace MyBet.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string databasePath)
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();

            services.AddDbContext<MyBetDbContext>(options => options.UseSqlite(connectionString));

            services.AddScoped<ISlipRepository, SlipRepository>();
            services.AddScoped<IScannedFileRepository, ScannedFileRepository>();
            services.AddScoped<IReviewQueueRepository, ReviewQueueRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDatabaseManager, DatabaseManager>();

            return services;
        }
    }
}
