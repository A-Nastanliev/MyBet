using MyBet.Application.Import;
using MyBet.Domain.Enums;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class ImportServiceTests : IDisposable
    {
        private readonly FakeSlipExtractor _extractor = new();
        private readonly InMemorySlipRepository _slips = new();
        private readonly InMemoryScannedFileRepository _scannedFiles = new();
        private readonly InMemoryReviewQueueRepository _reviewQueue = new();
        private readonly FakeUnitOfWork _unitOfWork = new();
        private readonly TempDirectory _dir = new();
        private readonly ImportService _service;

        public ImportServiceTests()
        {
            _service = new ImportService(_extractor, _slips, _scannedFiles, _reviewQueue, _unitOfWork);
        }

        public void Dispose() => _dir.Dispose();

        [Fact]
        public async Task Valid_slip_is_saved_and_its_file_recorded()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Equal("1001", result.SlipId);
            Assert.Equal("1001", Assert.Single(_slips.Slips).SlipId);
            Assert.Equal(ScanOutcome.Saved, Assert.Single(_scannedFiles.Files).Outcome);
            Assert.Empty(_reviewQueue.Items);
            Assert.Equal(1, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Cashed_out_slip_is_saved_without_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.CashedOut("1009"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
            Assert.Equal(SlipStatus.CashedOut, Assert.Single(_slips.Slips).Status);
        }

        [Fact]
        public async Task Lost_slip_is_saved_with_a_zero_payout()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.Lost("1008"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Empty(result.Issues);
            var saved = Assert.Single(_slips.Slips);
            Assert.Equal(SlipStatus.Lost, saved.Status);
            Assert.Equal(0m, saved.Payout);
        }

        [Fact]
        public async Task Slip_with_undetermined_status_goes_to_the_review_queue_and_not_to_the_slips()
        {
            var path = _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.Undetermined("1010"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.NeedsReview, result.Outcome);
            Assert.Equal("1010", result.SlipId);
            Assert.NotEmpty(result.Issues);
            Assert.Empty(_slips.Slips);

            var item = Assert.Single(_reviewQueue.Items);
            Assert.Equal(path, item.FilePath);
            Assert.Equal("1010", item.Extracted.SlipId);
            Assert.Equal(4.10m, item.Extracted.Payout);
            Assert.Equal(ScanOutcome.Queued, Assert.Single(_scannedFiles.Files).Outcome);
            Assert.Equal(item.FileHash, _scannedFiles.Files[0].Hash);
        }

        [Fact]
        public async Task Slip_without_a_reference_goes_to_the_review_queue()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.NoReference());

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.NeedsReview, result.Outcome);
            Assert.Null(result.SlipId);
            Assert.Empty(_slips.Slips);
            Assert.Single(_reviewQueue.Items);
        }

        [Fact]
        public async Task Duplicate_slip_is_not_saved_but_its_file_is_recorded()
        {
            _slips.Add(TestSlips.Saved("1001"));
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Duplicate, Assert.Single(results).Outcome);
            Assert.Single(_slips.Slips);
            Assert.Equal(ScanOutcome.Duplicate, Assert.Single(_scannedFiles.Files).Outcome);
        }

        [Fact]
        public async Task Slip_reference_is_compared_exactly_including_case()
        {
            _slips.Add(TestSlips.Saved("AbC=="));
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("abc=="));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
            Assert.Equal(2, _slips.Slips.Count);
        }

        [Fact]
        public async Task Null_extraction_is_failed_and_nothing_is_recorded()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(null);

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Failed, Assert.Single(results).Outcome);
            Assert.Empty(_slips.Slips);
            Assert.Empty(_reviewQueue.Items);
            Assert.Empty(_scannedFiles.Files);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task One_failing_file_does_not_stop_the_run()
        {
            _dir.AddFile("a.png");
            _dir.AddFile("b.png");
            _extractor.EnqueueFailure(new InvalidOperationException("Ollama is down"));
            _extractor.Enqueue(TestSlips.ValidDouble("1004"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(2, results.Count);
            Assert.Equal(ImportOutcome.Failed, results[0].Outcome);
            Assert.Contains("Ollama is down", results[0].Issues);
            Assert.Equal(ImportOutcome.Imported, results[1].Outcome);
        }

        [Fact]
        public async Task Failed_file_is_tried_again_on_the_next_run()
        {
            _dir.AddFile("a.png");
            _extractor.EnqueueFailure(new InvalidOperationException("Ollama is down"));
            await _service.ImportDirectoryAsync(_dir.FullPath);

            _extractor.Enqueue(TestSlips.ValidDouble("1004"));
            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
            Assert.Equal(2, _extractor.CallCount);
        }

        [Fact]
        public async Task Scanned_file_is_skipped_without_calling_the_model()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));
            await _service.ImportDirectoryAsync(_dir.FullPath);

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.AlreadyScanned, Assert.Single(results).Outcome);
            Assert.Equal(1, _extractor.CallCount);
            Assert.Single(_slips.Slips);
        }

        [Fact]
        public async Task Queued_file_is_not_sent_to_the_model_again()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.Undetermined("1010"));
            await _service.ImportDirectoryAsync(_dir.FullPath);

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.AlreadyScanned, Assert.Single(results).Outcome);
            Assert.Equal(1, _extractor.CallCount);
            Assert.Single(_reviewQueue.Items);
        }

        [Fact]
        public async Task Files_with_identical_content_are_scanned_once()
        {
            var content = new byte[] { 9, 9, 9 };
            _dir.AddFile("a.png", content);
            _dir.AddFile("b.png", content);
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, results[0].Outcome);
            Assert.Equal(ImportOutcome.AlreadyScanned, results[1].Outcome);
            Assert.Equal(1, _extractor.CallCount);
        }

        [Fact]
        public async Task Non_image_files_are_ignored()
        {
            _dir.AddFile("notes.txt");
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1005"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Single(results);
            Assert.Equal(1, _extractor.CallCount);
        }
    }
}
