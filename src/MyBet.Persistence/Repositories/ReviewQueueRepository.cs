using Microsoft.EntityFrameworkCore;
using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Persistence.Mapping;

namespace MyBet.Persistence.Repositories
{
    public class ReviewQueueRepository : IReviewQueueRepository
    {
        private readonly MyBetDbContext _db;

        public ReviewQueueRepository(MyBetDbContext db) => _db = db;

        public void Add(ReviewItem item) => _db.ReviewQueue.Add(item.ToRecord());

        public async Task<IReadOnlyList<ReviewItem>> GetAllAsync(CancellationToken ct = default)
        {
            var records = await _db.ReviewQueue.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);
            return records.Select(r => r.ToDomain()).ToList();
        }

        public Task<int> CountAsync(CancellationToken ct = default) => _db.ReviewQueue.CountAsync(ct);
    }
}
