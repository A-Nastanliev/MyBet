using MyBet.Application.Review;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;
using MyBet.Tests.Support;

namespace MyBet.Tests.Persistence
{
    public class ReviewServicePersistenceTests : IDisposable
    {
        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly SlipRepository _slips;
        private readonly ReviewQueueRepository _queue;
        private readonly UnitOfWork _unitOfWork;
        private readonly ReviewService _service;

        public ReviewServicePersistenceTests()
        {
            _context = _db.NewContext();
            _slips = new SlipRepository(_context);
            _queue = new ReviewQueueRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
            _service = new ReviewService(_queue, _slips, _unitOfWork);
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
        }

        private static ReviewEdit WonEdit(string slipId = "1001") => new(slipId, 1m, 2.58m, 2.63m, 3m);

        private async Task<int> QueueItemAsync(string hash = "h1", string slipId = "1001")
        {
            _queue.Add(new ReviewItem
            {
                FileHash = hash,
                FilePath = $@"C:\Slips\{hash}.png",
                Extracted = new ExtractedSlip { SlipId = slipId, Stake = 1m, TotalOdds = 2.58m, BonusPercent = 3m }
            });
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            return (await new ReviewQueueRepository(other).GetAllAsync()).First(i => i.FileHash == hash).Id;
        }

        private async Task<(IReadOnlyList<Slip> Slips, IReadOnlyList<ReviewItem> Queue)> ReadAllAsync()
        {
            using var other = _db.NewContext();
            return (await new SlipRepository(other).GetAllAsync(), await new ReviewQueueRepository(other).GetAllAsync());
        }

        [Fact]
        public async Task Accept_saves_the_slip_and_deletes_the_queue_row_together()
        {
            var id = await QueueItemAsync();

            var result = await _service.AcceptAsync(id, WonEdit());

            Assert.Equal(ReviewOutcome.Saved, result.Outcome);
            var (slips, queue) = await ReadAllAsync();
            var saved = Assert.Single(slips);
            Assert.Equal("1001", saved.SlipId);
            Assert.Equal(2.63m, saved.Payout);
            Assert.Equal(SlipStatus.Won, saved.Status);
            Assert.Empty(queue);
        }

        [Fact]
        public async Task Accept_only_removes_the_accepted_row()
        {
            var first = await QueueItemAsync("h1", "1001");
            await QueueItemAsync("h2", "1002");

            await _service.AcceptAsync(first, WonEdit("1001"));

            var (_, queue) = await ReadAllAsync();
            Assert.Equal("h2", Assert.Single(queue).FileHash);
        }

        [Fact]
        public async Task Accept_of_a_saved_slip_reference_is_rejected_and_the_row_stays()
        {
            _slips.Add(TestSlips.Saved("1001"));
            await _unitOfWork.SaveChangesAsync();
            var id = await QueueItemAsync("h1", "1001");

            var result = await _service.AcceptAsync(id, WonEdit("1001"));

            Assert.Equal(ReviewOutcome.Duplicate, result.Outcome);
            var (slips, queue) = await ReadAllAsync();
            Assert.Single(slips);
            Assert.Single(queue);
        }

        [Fact]
        public async Task Accept_that_is_still_undetermined_keeps_the_row()
        {
            var id = await QueueItemAsync();

            var result = await _service.AcceptAsync(id, WonEdit() with { Payout = null });

            Assert.Equal(ReviewOutcome.StillUndetermined, result.Outcome);
            var (slips, queue) = await ReadAllAsync();
            Assert.Empty(slips);
            Assert.Single(queue);
        }

        [Fact]
        public async Task Discard_deletes_the_row_without_saving_a_slip()
        {
            var id = await QueueItemAsync();

            Assert.True(await _service.DiscardAsync(id));

            var (slips, queue) = await ReadAllAsync();
            Assert.Empty(slips);
            Assert.Empty(queue);
        }

        [Fact]
        public async Task Accept_of_a_missing_row_is_not_found()
        {
            var result = await _service.AcceptAsync(99, WonEdit());

            Assert.Equal(ReviewOutcome.NotFound, result.Outcome);
            var (slips, _) = await ReadAllAsync();
            Assert.Empty(slips);
        }
    }
}