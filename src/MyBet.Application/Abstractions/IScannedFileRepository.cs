using MyBet.Domain.Entities;

namespace MyBet.Application.Abstractions
{
    public interface IScannedFileRepository
    {
        Task<bool> ExistsAsync(string hash, CancellationToken ct = default);
        void Add(ScannedFile file);
    }
}
