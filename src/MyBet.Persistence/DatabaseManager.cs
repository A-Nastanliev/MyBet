using Microsoft.EntityFrameworkCore;
using MyBet.Application.Abstractions;

namespace MyBet.Persistence
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly MyBetDbContext _db;

        public DatabaseManager(MyBetDbContext db) => _db = db;

        public async Task EnsureCreatedAsync(CancellationToken ct = default) =>
            await _db.Database.EnsureCreatedAsync(ct);

        public async Task ResetAsync(CancellationToken ct = default)
        {
            _db.ChangeTracker.Clear();
            await _db.Database.EnsureDeletedAsync(ct);
            await _db.Database.EnsureCreatedAsync(ct);
        }
    }
}
