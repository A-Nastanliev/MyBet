using Microsoft.EntityFrameworkCore;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;
using MyBet.Tests.Support;

namespace MyBet.Tests.Persistence
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly SlipRepository _slips;
        private readonly ScannedFileRepository _scannedFiles;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            _context = _db.NewContext();
            _slips = new SlipRepository(_context);
            _scannedFiles = new ScannedFileRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
        }

        private static ScannedFile File(string hash) =>
            new() { Hash = hash, ScannedAt = DateTimeOffset.UtcNow, Outcome = ScanOutcome.Saved };

        private async Task<bool> SlipExistsInDatabaseAsync(string slipId)
        {
            using var other = _db.NewContext();
            return await new SlipRepository(other).ExistsAsync(slipId);
        }

        [Fact]
        public async Task A_slip_and_its_file_are_saved_together()
        {
            _slips.Add(TestSlips.Saved("1001"));
            _scannedFiles.Add(File("AAAA"));

            await _unitOfWork.SaveChangesAsync();

            Assert.True(await SlipExistsInDatabaseAsync("1001"));
            using var other = _db.NewContext();
            Assert.True(await new ScannedFileRepository(other).ExistsAsync("AAAA"));
        }

        [Fact]
        public async Task When_one_part_fails_nothing_is_saved()
        {
            _scannedFiles.Add(File("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            _slips.Add(TestSlips.Saved("1001"));
            _scannedFiles.Add(File("AAAA"));

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());
            Assert.False(await SlipExistsInDatabaseAsync("1001"));
        }

        [Fact]
        public async Task A_failed_save_does_not_break_the_next_one()
        {
            _scannedFiles.Add(File("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            _slips.Add(TestSlips.Saved("1001"));
            _scannedFiles.Add(File("AAAA"));
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());

            _slips.Add(TestSlips.Saved("1002"));
            _scannedFiles.Add(File("BBBB"));
            await _unitOfWork.SaveChangesAsync();

            Assert.False(await SlipExistsInDatabaseAsync("1001"));
            Assert.True(await SlipExistsInDatabaseAsync("1002"));
        }
    }
}
