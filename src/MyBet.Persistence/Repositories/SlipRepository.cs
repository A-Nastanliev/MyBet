using Microsoft.EntityFrameworkCore;
using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Persistence.Mapping;

namespace MyBet.Persistence.Repositories
{
    public class SlipRepository : ISlipRepository
    {
        private readonly MyBetDbContext _db;

        public SlipRepository(MyBetDbContext db) => _db = db;

        public Task<bool> ExistsAsync(string slipId, CancellationToken ct = default) =>
            _db.Slips.AnyAsync(s => s.SlipId == slipId, ct);

        public void Add(Slip slip) => _db.Slips.Add(slip.ToRecord());

        public async Task<IReadOnlyList<Slip>> GetAllAsync(CancellationToken ct = default)
        {
            var records = await _db.Slips.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);
            return records.Select(r => r.ToDomain()).ToList();
        }
    }
}
