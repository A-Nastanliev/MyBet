using MyBet.Application.Import;
using MyBet.Domain.Enums;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class ImportServiceTests : IDisposable
    {
        private readonly FakeSlipExtractor _extractor = new();
        private readonly InMemorySlipRepository _repository = new();
        private readonly TempDirectory _dir = new();
        private readonly ImportService _service;

        public ImportServiceTests()
        {
            _service = new ImportService(_extractor, _repository);
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
        public async Task Results_and_status_from_the_extractor_are_saved_unchanged()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.LostWithPendingLegs("1006"));

            await _service.ImportDirectoryAsync(_dir.FullPath);

            var saved = Assert.Single(_repository.Slips);
            Assert.Equal(SlipStatus.Lost, saved.Status);
            Assert.Equal(
                new LegResult?[] { LegResult.Lost, LegResult.Pending, LegResult.Pending },
                saved.Legs.Select(l => l.Result).ToArray());
        }

        [Fact]
        public async Task Won_slip_with_an_early_paid_leg_is_imported_without_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.WonWithEarlyPayout("1007"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
            var saved = Assert.Single(_repository.Slips);
            Assert.Equal(SlipStatus.Won, saved.Status);
            Assert.Equal(LegResult.EarlyPayout, saved.Legs[1].Result);
        }

        [Fact]
        public async Task Lost_slip_with_pending_legs_is_imported_without_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.LostWithPendingLegs("1008"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public async Task Cashed_out_slip_with_a_pending_leg_is_imported_without_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.CashedOutWithPendingLeg("1009"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            Assert.Equal(ImportOutcome.Imported, Assert.Single(results).Outcome);
            Assert.Equal(SlipStatus.CashedOut, Assert.Single(_repository.Slips).Status);
        }

        [Fact]
        public async Task Pending_leg_without_a_lost_leg_or_cash_out_is_sent_to_review()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.WonLegAndPendingLeg("1010"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var result = Assert.Single(results);
            Assert.Equal(ImportOutcome.NeedsReview, result.Outcome);
            Assert.Contains(result.Issues, i => i.Contains("pending"));
            var saved = Assert.Single(_repository.Slips);
            Assert.Null(saved.Status);
            Assert.True(saved.NeedsReview);
        }

        [Fact]
        public async Task Import_does_not_override_a_result_with_the_score()
        {
            var slip = TestSlips.DoubleWithAstonVillaOver("1011");
            slip.Legs[0].Result = LegResult.Won;
            slip.Legs[1].Result = LegResult.Won;
            slip.Status = SlipStatus.Won;
            _dir.AddFile("a.png");
            _extractor.Enqueue(slip);

            await _service.ImportDirectoryAsync(_dir.FullPath);

            var saved = Assert.Single(_repository.Slips);
            Assert.Equal(LegResult.Won, saved.Legs[1].Result);
            Assert.Equal(SlipStatus.Won, saved.Status);
        }

        [Fact]
        public async Task Import_does_not_guess_a_missing_result_from_the_score()
        {
            _dir.AddFile("a.png");
            _extractor.Enqueue(TestSlips.DoubleWithAstonVillaOver("1012"));

            var results = await _service.ImportDirectoryAsync(_dir.FullPath);

            var saved = Assert.Single(_repository.Slips);
            Assert.All(saved.Legs, l => Assert.Null(l.Result));
            Assert.Null(saved.Status);
            Assert.Equal(ImportOutcome.NeedsReview, Assert.Single(results).Outcome);
        }
    }
}