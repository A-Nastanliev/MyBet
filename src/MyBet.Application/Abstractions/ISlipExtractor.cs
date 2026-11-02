using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface ISlipExtractor
    {
        Task<Slip?> ExtractAsync(byte[] imageBytes, CancellationToken ct = default);
    }
}
