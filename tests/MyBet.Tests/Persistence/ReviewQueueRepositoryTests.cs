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

        private static ReviewItem Item(string hash, ExtractedSlip? extracted = null) => new()
        {
            FileHash = hash,
            FilePath = @"D:\Slips\" + hash + ".png",
            Extracted = extracted ?? new ExtractedSlip { SlipId = "1010", Stake = 2m, Payout = 4.10m }
        };

        [Fact]
        public async Task Item_is_stored_with_the_values_the_model_read()
        {
            _repository.Add(Item("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            var item = Assert.Single(await new ReviewQueueRepository(other).GetAllAsync());

            Assert.True(item.Id > 0);
            Assert.Equal("AAAA", item.FileHash);
            Assert.Equal(@"D:\Slips\AAAA.png", item.FilePath);
            Assert.Equal("1010", item.Extracted.SlipId);
            Assert.Equal(2m, item.Extracted.Stake);
            Assert.Equal(4.10m, item.Extracted.Payout);
        }

        [Fact]
        public async Task Fields_the_model_could_not_read_stay_null()
        {
            _repository.Add(Item("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            var item = Assert.Single(await new ReviewQueueRepository(other).GetAllAsync());

            Assert.Null(item.Extracted.TotalOdds);
            Assert.Null(item.Extracted.BonusPercent);
            Assert.Null(item.Extracted.Status);
            Assert.True(item.Extracted.NeedsReview);
        }

        [Fact]
        public async Task Item_without_a_slip_reference_can_be_stored()
        {
            _repository.Add(Item("AAAA", new ExtractedSlip { SlipId = "", Stake = 1m, TotalOdds = 2m }));
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            var item = Assert.Single(await new ReviewQueueRepository(other).GetAllAsync());

            Assert.Equal("", item.Extracted.SlipId);
        }

        [Fact]
        public async Task Items_are_counted_and_listed_in_the_order_they_were_saved()
        {
            _repository.Add(Item("BBBB"));
            _repository.Add(Item("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            Assert.Equal(2, await _repository.CountAsync());
            Assert.Equal(new[] { "BBBB", "AAAA" }, (await _repository.GetAllAsync()).Select(i => i.FileHash));
        }

        [Fact]
        public async Task Same_file_twice_is_rejected_by_the_database()
        {
            _repository.Add(Item("AAAA"));
            await _unitOfWork.SaveChangesAsync();

            _repository.Add(Item("AAAA"));

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());
        }
    }
}
