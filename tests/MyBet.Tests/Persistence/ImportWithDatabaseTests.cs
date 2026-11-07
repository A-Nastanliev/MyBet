using MyBet.Application.Import;
using MyBet.Domain.Enums;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Persistence
{
    public class ImportWithDatabaseTests : IDisposable
    {
        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly FakeSlipExtractor _extractor = new();
        private readonly TempDirectory _dir = new();
        private readonly ImportService _service;

        public ImportWithDatabaseTests()
        {
            _context = _db.NewContext();
            _service = new ImportService(
                _extractor,
                new SlipRepository(_context),
                new ScannedFileRepository(_context),
                new ReviewQueueRepository(_context),
                new UnitOfWork(_context));
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
            _dir.Dispose();
        }

        [Fact]
        public async Task Imported_slip_is_in_the_database()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            await _service.ImportDirectoryAsync(_dir.FullPath);

            using var other = _db.NewContext();
            var saved = Assert.Single(await new SlipRepository(other).GetAllAsync());
            Assert.Equal("1001", saved.SlipId);
            Assert.Equal(SlipStatus.Won, saved.Status);
        }

        [Fact]
        public async Task Slip_that_needs_review_is_queued_and_not_saved_and_not_scanned_again()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.Undetermined("1010"));

            await _service.ImportDirectoryAsync(_dir.FullPath);
            var second = await _service.ImportDirectoryAsync(_dir.FullPath);

            using var other = _db.NewContext();
            Assert.Empty(await new SlipRepository(other).GetAllAsync());
            Assert.Equal(1, await new ReviewQueueRepository(other).CountAsync());
            Assert.Equal(ImportOutcome.AlreadyScanned, Assert.Single(second).Outcome);
            Assert.Equal(1, _extractor.CallCount);
        }

        [Fact]
        public async Task Same_slip_in_two_screenshots_is_saved_once()
        {
            _dir.AddFile("a.png");
            _dir.AddFile("b.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, results[0].Outcome);
            Assert.Equal(ImportOutcome.Duplicate, results[1].Outcome);
            using var other = _db.NewContext();
            Assert.Single(await new SlipRepository(other).GetAllAsync());
        }
    }
}
