using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBet.Application.Abstractions;
using MyBet.Persistence;
using MyBet.Tests.Support;

namespace MyBet.Tests.Persistence
{
    public class DatabaseManagerTests : IDisposable
    {
        private readonly TempDirectory _dir = new();
        private readonly string _path;

        public DatabaseManagerTests() => _path = Path.Combine(_dir.FullPath, "mybet.db");

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            _dir.Dispose();
        }

        private ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddPersistence(_path);
            return services.BuildServiceProvider();
        }

        private static async Task AddSlipAsync(IServiceProvider sp, string slipId)
        {
            sp.GetRequiredService<ISlipRepository>().Add(TestSlips.Saved(slipId));
            await sp.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        [Fact]
        public async Task Database_file_is_created_with_its_tables()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();

            Assert.True(File.Exists(_path));
            Assert.Empty(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
            Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<IReviewQueueRepository>().CountAsync());
        }

        [Fact]
        public async Task Fresh_database_has_every_migration_applied_and_no_backup()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();

            var context = scope.ServiceProvider.GetRequiredService<MyBetDbContext>();
            Assert.Empty(await context.Database.GetPendingMigrationsAsync());
            Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
            Assert.False(File.Exists(_path + ".bak"));
        }

        [Fact]
        public async Task Migrating_twice_keeps_the_data()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();
                await AddSlipAsync(scope.ServiceProvider, "1001");
            }

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();
                Assert.Single(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
            }

            Assert.False(File.Exists(_path + ".bak"));
        }

        [Fact]
        public async Task Database_made_with_EnsureCreated_is_adopted_with_its_data_and_backed_up()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<MyBetDbContext>().Database.EnsureCreatedAsync();
                await AddSlipAsync(scope.ServiceProvider, "1001");
            }

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();

                Assert.Single(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());

                var context = scope.ServiceProvider.GetRequiredService<MyBetDbContext>();
                Assert.Empty(await context.Database.GetPendingMigrationsAsync());
                Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
            }

            Assert.True(File.Exists(_path + ".bak"));
        }

        [Fact]
        public async Task Adopting_an_old_database_twice_is_harmless()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<MyBetDbContext>().Database.EnsureCreatedAsync();
                await AddSlipAsync(scope.ServiceProvider, "1001");
            }

            for (var i = 0; i < 2; i++)
            {
                using var scope = provider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();
            }

            using (var scope = provider.CreateScope())
                Assert.Single(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
        }

        [Fact]
        public async Task Reset_leaves_empty_up_to_date_tables()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().MigrateAsync();
                await AddSlipAsync(scope.ServiceProvider, "1001");
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().ResetAsync();
            }

            using (var scope = provider.CreateScope())
            {
                Assert.Empty(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
                Assert.False(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().ExistsAsync("1001"));
                Assert.Empty(await scope.ServiceProvider.GetRequiredService<MyBetDbContext>()
                    .Database.GetPendingMigrationsAsync());
            }
        }
    }
}
