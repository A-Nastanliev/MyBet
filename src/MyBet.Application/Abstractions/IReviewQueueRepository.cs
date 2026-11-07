using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface IReviewQueueRepository
    {
        void Add(ReviewItem item);

        Task<IReadOnlyList<ReviewItem>> GetAllAsync(CancellationToken ct = default);

        Task<int> CountAsync(CancellationToken ct = default);
    }
}
