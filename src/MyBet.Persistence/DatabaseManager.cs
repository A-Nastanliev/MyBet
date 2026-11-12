using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyBet.Application.Abstractions;

namespace MyBet.Persistence
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly MyBetDbContext _db;

        public DatabaseManager(MyBetDbContext db) => _db = db;

        public async Task MigrateAsync(CancellationToken ct = default)
        {
            var applied = (await _db.Database.GetAppliedMigrationsAsync(ct)).ToList();
            var pending = (await _db.Database.GetPendingMigrationsAsync(ct)).ToList();

            var legacy = applied.Count == 0 && await TableExistsAsync("Slips", ct);

            if (legacy || (applied.Count > 0 && pending.Count > 0))
                await BackupAsync(ct);

            if (legacy)
                await BaselineAsync(ct);

            await _db.Database.MigrateAsync(ct);
        }

        public async Task ResetAsync(CancellationToken ct = default)
        {
            _db.ChangeTracker.Clear();
            await _db.Database.EnsureDeletedAsync(ct);
            await _db.Database.MigrateAsync(ct);
        }

        private async Task<bool> TableExistsAsync(string name, CancellationToken ct)
        {
            var count = await _db.Database
                .SqlQuery<int>($"SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name = {name}")
                .SingleAsync(ct);

            return count > 0;
        }

        private async Task BaselineAsync(CancellationToken ct)
        {
            var first = _db.Database.GetMigrations().First();
            var version = (typeof(DbContext).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "10.0.0")
                .Split('+')[0];

            await _db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" +
                "\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, " +
                "\"ProductVersion\" TEXT NOT NULL)", ct);

            await _db.Database.ExecuteSqlAsync(
                $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({first}, {version})", ct);
        }

        private async Task BackupAsync(CancellationToken ct)
        {
            if (_db.Database.GetDbConnection() is not SqliteConnection source) return;

            await _db.Database.OpenConnectionAsync(ct);
            try
            {
                var path = source.DataSource;
                if (string.IsNullOrEmpty(path) || path == ":memory:" || !File.Exists(path)) return;

                var target = new SqliteConnectionStringBuilder { DataSource = path + ".bak", Pooling = false }.ToString();
                await using var backup = new SqliteConnection(target);
                await backup.OpenAsync(ct);
                source.BackupDatabase(backup);
            }
            finally
            {
                await _db.Database.CloseConnectionAsync();
            }
        }
    }
}