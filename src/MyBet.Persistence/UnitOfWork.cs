using MyBet.Application.Abstractions;

namespace MyBet.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyBetDbContext _db;

        public UnitOfWork(MyBetDbContext db) => _db = db;

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch
            {
                _db.ChangeTracker.Clear();
                throw;
            }
        }
    }
}
