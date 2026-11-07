using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface ISlipExtractor
    {
        Task<ExtractedSlip?> ExtractAsync(byte[] imageBytes, CancellationToken ct = default);
    }
}
