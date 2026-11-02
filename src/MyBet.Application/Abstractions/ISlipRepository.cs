using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface ISlipRepository
    {
        Task<bool> ExistsAsync(string slipId, CancellationToken ct = default);
        Task AddAsync(Slip slip, CancellationToken ct = default);
        Task<IReadOnlyList<Slip>> GetAllAsync(CancellationToken ct = default);
    }
}
