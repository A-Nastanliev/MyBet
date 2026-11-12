using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyBet.Persistence;

namespace MyBet.Tests.Persistence
{
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteTestDatabase()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            using var context = NewContext();
            context.Database.Migrate();
        }

        public MyBetDbContext NewContext() =>
            new(new DbContextOptionsBuilder<MyBetDbContext>().UseSqlite(_connection).Options);

        public void Dispose() => _connection.Dispose();
    }
}