using MyBet.Application.Import;
using MyBet.Domain.Enums;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class ImportServiceTests : IDisposable
    {
        private readonly FakeSlipExtractor _extractor = new();
        private readonly FakeBadgeReader _badgeReader = new();
        private readonly InMemorySlipRepository _repository = new();
        private readonly TempDirectory _dir = new();
        private readonly ImportService _service;

        public ImportServiceTests()
        {
            _service = new ImportService(_extractor, _badgeReader, _repository);
        }

        public void Dispose() => _dir.Dispose();

        [Fact]
        public async Task Valid_slip_is_imported_and_saved()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Equal("1001", result.SlipId);
            var saved = Assert.Single(_repository.Slips);
            Assert.False(saved.NeedsReview);
        }

        [Fact]
        public async Task Duplicate_slip_is_skipped()
        {
            await _repository.AddAsync(TestSlips.ValidDouble("1001"));
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1001"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Duplicate, Assert.Single(results).Outcome);
            Assert.Single(_repository.Slips);
        }

        [Fact]
        public async Task Slip_failing_validation_is_saved_as_needs_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.WithDroppedLeg("1002"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.NeedsReview, result.Outcome);
            Assert.NotEmpty(result.Issues);
            Assert.True(Assert.Single(_repository.Slips).NeedsReview);
        }

        [Fact]
        public async Task Null_extraction_is_reported_as_failed_and_not_saved()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(null);

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Failed, Assert.Single(results).Outcome);
            Assert.Empty(_repository.Slips);
        }

        [Fact]
        public async Task Badge_results_are_applied_to_legs_in_order()
        {
            _badgeReader.Results = new LegResult?[] { LegResult.Won, LegResult.Lost };
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1003"));

            await _service.ImportDirectoryAsync(_dir.FullPath);

            var legs = Assert.Single(_repository.Slips).Legs;
            Assert.Equal(LegResult.Won, legs[0].Result);
            Assert.Equal(LegResult.Lost, legs[1].Result);
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
        public async Task Non_image_files_are_ignored()
        {
            _dir.AddFile("notes.txt");
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.ValidDouble("1005"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Single(results);
            Assert.Equal(1, _extractor.CallCount);
        }

        [Fact]
        public async Task Leg_without_a_badge_is_decided_from_the_score_and_the_slip_is_lost()
        {
            _badgeReader.Results = new LegResult?[] { LegResult.Won, null };
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.DoubleWithAstonVillaOver("1006"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var saved = Assert.Single(_repository.Slips);
            Assert.Equal(LegResult.Lost, saved.Legs[1].Result);
            Assert.Equal(SlipStatus.Lost, saved.Status);
            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
        }

        [Fact]
        public async Task Badge_result_wins_over_the_score_evaluation()
        {
            _badgeReader.Results = new LegResult?[] { LegResult.Won, LegResult.Won };
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.DoubleWithAstonVillaOver("1007"));

            await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(LegResult.Won, Assert.Single(_repository.Slips).Legs[1].Result);
        }

        [Fact]
        public async Task Leg_that_cannot_be_decided_sends_the_slip_to_review()
        {
            _badgeReader.Results = new LegResult?[] { LegResult.Won, null };
            var slip = TestSlips.DoubleWithAstonVillaOver("1008");
            slip.Legs[1].Market = "Двойна Възможност";
            _dir.AddFile("a.png");
            _extractor.Enqueue(slip);

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var saved = Assert.Single(_repository.Slips);
            Assert.Null(saved.Legs[1].Result);
            Assert.Null(saved.Status);
            Assert.Equal(ImportOutcome.NeedsReview, Assert.Single(results).Outcome);
        }
    }
}
