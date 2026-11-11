using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;

namespace MyBet.Tests.Fakes
{
    public class InMemoryReviewQueueRepository : IReviewQueueRepository
    {
        private readonly List<ReviewItem> _items = new();

        public IReadOnlyList<ReviewItem> Items => _items;

        public void Add(ReviewItem item) => _items.Add(item);

        public Task<ReviewItem?> GetAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

        public void Remove(int id) => _items.RemoveAll(i => i.Id == id);

        public Task<IReadOnlyList<ReviewItem>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ReviewItem>>(_items.ToList());

        public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(_items.Count);
    }
}
