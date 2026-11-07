using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;

namespace MyBet.Tests.Fakes
{
    public class InMemorySlipRepository : ISlipRepository
    {
        private readonly List<Slip> _slips = new();

        public IReadOnlyList<Slip> Slips => _slips;

        public Task<bool> ExistsAsync(string slipId, CancellationToken ct = default) =>
            Task.FromResult(_slips.Any(s => s.SlipId == slipId));

        public void Add(Slip slip) => _slips.Add(slip);

        public Task<IReadOnlyList<Slip>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Slip>>(_slips.ToList());
    }
}
