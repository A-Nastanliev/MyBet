using Microsoft.EntityFrameworkCore;
using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Persistence.Mapping;

namespace MyBet.Persistence.Repositories
{
    public class ScannedFileRepository : IScannedFileRepository
    {
        private readonly MyBetDbContext _db;

        public ScannedFileRepository(MyBetDbContext db) => _db = db;

        public Task<bool> ExistsAsync(string hash, CancellationToken ct = default) =>
            _db.ScannedFiles.AnyAsync(f => f.Hash == hash, ct);

        public void Add(ScannedFile file) => _db.ScannedFiles.Add(file.ToRecord());
    }
}
