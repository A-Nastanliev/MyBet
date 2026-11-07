using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;

namespace MyBet.Tests.Fakes
{
    public class InMemoryScannedFileRepository : IScannedFileRepository
    {
        private readonly List<ScannedFile> _files = new();

        public IReadOnlyList<ScannedFile> Files => _files;

        public Task<bool> ExistsAsync(string hash, CancellationToken ct = default) =>
            Task.FromResult(_files.Any(f => f.Hash == hash));

        public void Add(ScannedFile file) => _files.Add(file);
    }
}
