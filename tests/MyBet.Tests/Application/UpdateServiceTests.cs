using MyBet.Application.Import;
using MyBet.Application.Update;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class UpdateServiceTests : IDisposable
    {
        private readonly FakeSettingsStore _settings = new();
        private readonly FakeSlipExtractor _extractor = new();
        private readonly InMemoryReviewQueueRepository _reviewQueue = new();
        private readonly TempDirectory _dir = new();
        private readonly UpdateService _service;

        public UpdateServiceTests()
        {
            var import = new ImportService(
                _extractor,
                new InMemorySlipRepository(),
                new InMemoryScannedFileRepository(),
                _reviewQueue,
                new FakeUnitOfWork());

            _service = new UpdateService(_settings, import, _reviewQueue);
        }

        public void Dispose() => _dir.Dispose();

        [Fact]
        public async Task No_home_folder_does_nothing()
        {
            var result = await _service.RunAsync();

            Assert.Equal(UpdateStatus.NoHomeFolder, result.Status);
            Assert.Empty(result.Imports);
        }

        [Fact]
        public async Task Missing_home_folder_does_nothing()
        {
            _settings.Folder = Path.Combine(_dir.FullPath, "gone");

            var result = await _service.RunAsync();

            Assert.Equal(UpdateStatus.FolderMissing, result.Status);
            Assert.Empty(result.Imports);
        }

        [Fact]
        public async Task New_files_are_imported_and_the_review_queue_is_counted()
        {
            _settings.Folder = _dir.FullPath;
            _dir.AddFile("a.png");
            _dir.AddFile("b.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));
            _extractor.Enqueue(TestSlips.Undetermined("1002"));

            var result = await _service.RunAsync();

            Assert.Equal(UpdateStatus.Completed, result.Status);
            Assert.Equal(1, result.CountOf(ImportOutcome.Imported));
            Assert.Equal(1, result.CountOf(ImportOutcome.NeedsReview));
            Assert.Equal(1, result.ReviewQueueCount);
        }

        [Fact]
        public async Task Progress_is_reported_once_per_file()
        {
            _settings.Folder = _dir.FullPath;
            _dir.AddFile("a.png");
            _dir.AddFile("b.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));
            _extractor.Enqueue(TestSlips.ValidDouble("1002"));
            var progress = new RecordingProgress<ImportProgress>();

            await _service.RunAsync(progress);

            Assert.Equal(new[] { 1, 2 }, progress.Reports.Select(p => p.Done));
            Assert.All(progress.Reports, p => Assert.Equal(2, p.Total));
        }
    }
}