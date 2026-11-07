using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface ISlipRepository
    {
        Task<bool> ExistsAsync(string slipId, CancellationToken ct = default);

        void Add(Slip slip);

        Task<IReadOnlyList<Slip>> GetAllAsync(CancellationToken ct = default);
    }
}
