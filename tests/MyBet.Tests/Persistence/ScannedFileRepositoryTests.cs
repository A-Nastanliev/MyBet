using Microsoft.EntityFrameworkCore;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;

namespace MyBet.Tests.Persistence
{
    public class ScannedFileRepositoryTests : IDisposable
    {
        private static readonly DateTimeOffset Moment = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly ScannedFileRepository _repository;
        private readonly UnitOfWork _unitOfWork;

        public ScannedFileRepositoryTests()
        {
            _context = _db.NewContext();
            _repository = new ScannedFileRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
        }

        private static ScannedFile File(string hash, ScanOutcome outcome = ScanOutcome.Saved) =>
            new() { Hash = hash, ScannedAt = Moment, Outcome = outcome };

        [Fact]
        public async Task Hash_is_known_only_after_save()
        {
            _repository.Add(File("AAAA"));
            using (var before = _db.NewContext())
                Assert.False(await new ScannedFileRepository(before).ExistsAsync("AAAA"));

            await _unitOfWork.SaveChangesAsync();

            using var after = _db.NewContext();
            Assert.True(await new ScannedFileRepository(after).ExistsAsync("AAAA"));
            Assert.False(await new ScannedFileRepository(after).ExistsAsync("BBBB"));
        }

        [Fact]
        public async Task Outcome_and_time_are_stored()
        {
            _repository.Add(File("AAAA", ScanOutcome.Queued));
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            var record = await other.ScannedFiles.SingleAsync();

            Assert.Equal("AAAA", record.Hash);
            Assert.Equal(ScanOutcome.Queued, record.Outcome);
            Assert.Equal(Moment, record.ScannedAt);
        }

        [Fact]
        public async Task Same_hash_twice_is_rejected_by_the_database()
        {
            _repository.Add(File("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            _repository.Add(File("AAAA", ScanOutcome.Duplicate));

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());
        }
    }
}
