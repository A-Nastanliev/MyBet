using MyBet.Application.Abstractions;

namespace MyBet.Tests.Fakes
{
    public class FakeDatabaseManager : IDatabaseManager
    {
        public int ResetCount { get; private set; }

        public Task MigrateAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task ResetAsync(CancellationToken ct = default)
        {
            ResetCount++;
            return Task.CompletedTask;
        }
    }
}