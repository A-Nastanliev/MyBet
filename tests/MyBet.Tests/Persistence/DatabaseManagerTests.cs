using Microsoft.Data.Sqlite;
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

        [Fact]
        public async Task Database_file_is_created_with_its_tables()
        {
            using var provider = BuildProvider();
            using var scope = provider.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().EnsureCreatedAsync();

            Assert.True(File.Exists(_path));
            Assert.Empty(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
            Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<IReviewQueueRepository>().CountAsync());
        }

        [Fact]
        public async Task Creating_twice_keeps_the_data()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().EnsureCreatedAsync();
                scope.ServiceProvider.GetRequiredService<ISlipRepository>().Add(TestSlips.Saved("1001"));
                await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
            }

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().EnsureCreatedAsync();
                Assert.Single(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
            }
        }

        [Fact]
        public async Task Reset_leaves_empty_tables()
        {
            using var provider = BuildProvider();

            using (var scope = provider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().EnsureCreatedAsync();
                scope.ServiceProvider.GetRequiredService<ISlipRepository>().Add(TestSlips.Saved("1001"));
                await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
                await scope.ServiceProvider.GetRequiredService<IDatabaseManager>().ResetAsync();
            }

            using (var scope = provider.CreateScope())
            {
                Assert.Empty(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().GetAllAsync());
                Assert.False(await scope.ServiceProvider.GetRequiredService<ISlipRepository>().ExistsAsync("1001"));
            }
        }
    }
}
