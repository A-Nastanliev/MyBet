using Microsoft.EntityFrameworkCore;
using MyBet.Domain.Entities;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;

namespace MyBet.Tests.Persistence
{
    public class ReviewQueueRepositoryTests : IDisposable
    {
        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly ReviewQueueRepository _repository;
        private readonly UnitOfWork _unitOfWork;

        public ReviewQueueRepositoryTests()
        {
            _context = _db.NewContext();
            _repository = new ReviewQueueRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
        }

        private static ReviewItem Item(string hash, string slipId = "1001") => new()
        {
            FileHash = hash,
            FilePath = $@"C:\Slips\{hash}.png",
            Extracted = new ExtractedSlip
            {
                SlipId = slipId,
                Stake = 2.31m,
                TotalOdds = 14.16m,
                Payout = null,
                BonusPercent = 10m
            }
        };

        private async Task<IReadOnlyList<ReviewItem>> ReadAllAsync()
        {
            using var other = _db.NewContext();
            return await new ReviewQueueRepository(other).GetAllAsync();
        }

        [Fact]
        public async Task Item_is_stored_only_when_changes_are_saved()
        {
            _repository.Add(Item("h1"));
            Assert.Empty(await ReadAllAsync());

            await _unitOfWork.SaveChangesAsync();

            Assert.Single(await ReadAllAsync());
        }

        [Fact]
        public async Task All_fields_survive_a_round_trip_and_a_missing_payout_stays_null()
        {
            _repository.Add(Item("h1", "FbOAhzuOw148hoXv1eE8xQ=="));
            await _unitOfWork.SaveChangesAsync();

            var saved = Assert.Single(await ReadAllAsync());

            Assert.True(saved.Id > 0);
            Assert.Equal("h1", saved.FileHash);
            Assert.Equal(@"C:\Slips\h1.png", saved.FilePath);
            Assert.Equal("FbOAhzuOw148hoXv1eE8xQ==", saved.Extracted.SlipId);
            Assert.Equal(2.31m, saved.Extracted.Stake);
            Assert.Equal(14.16m, saved.Extracted.TotalOdds);
            Assert.Null(saved.Extracted.Payout);
            Assert.Equal(10m, saved.Extracted.BonusPercent);
        }

        [Fact]
        public async Task Get_returns_the_item_by_id_and_null_for_a_missing_one()
        {
            _repository.Add(Item("h1"));
            await _unitOfWork.SaveChangesAsync();
            var id = Assert.Single(await ReadAllAsync()).Id;

            using var other = _db.NewContext();
            var repository = new ReviewQueueRepository(other);

            Assert.Equal("h1", (await repository.GetAsync(id))?.FileHash);
            Assert.Null(await repository.GetAsync(id + 100));
        }

        [Fact]
        public async Task Item_added_and_saved_can_be_removed_through_the_same_context()
        {
            _repository.Add(Item("h1"));
            await _unitOfWork.SaveChangesAsync();
            var id = Assert.Single(await ReadAllAsync()).Id;

            _repository.Remove(id);
            await _unitOfWork.SaveChangesAsync();

            Assert.Empty(await ReadAllAsync());
        }

        [Fact]
        public async Task Item_can_be_removed_through_a_fresh_context()
        {
            _repository.Add(Item("h1"));
            await _unitOfWork.SaveChangesAsync();
            var id = Assert.Single(await ReadAllAsync()).Id;

            using var other = _db.NewContext();
            new ReviewQueueRepository(other).Remove(id);
            await new UnitOfWork(other).SaveChangesAsync();

            Assert.Empty(await ReadAllAsync());
        }

        [Fact]
        public async Task Remove_only_deletes_that_item()
        {
            _repository.Add(Item("h1", "1001"));
            _repository.Add(Item("h2", "1002"));
            await _unitOfWork.SaveChangesAsync();
            var first = (await ReadAllAsync()).First(i => i.FileHash == "h1");

            _repository.Remove(first.Id);
            await _unitOfWork.SaveChangesAsync();

            Assert.Equal("h2", Assert.Single(await ReadAllAsync()).FileHash);
            Assert.Equal(1, await _repository.CountAsync());
        }

        [Fact]
        public async Task Same_file_hash_twice_is_rejected_by_the_database()
        {
            _repository.Add(Item("h1"));
            await _unitOfWork.SaveChangesAsync();

            _repository.Add(Item("h1", "1002"));

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());
        }
    }
}