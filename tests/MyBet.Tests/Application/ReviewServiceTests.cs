using MyBet.Application.Review;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Tests.Fakes;

namespace MyBet.Tests.Application
{
    public class ReviewServiceTests
    {
        private readonly InMemorySlipRepository _slips = new();
        private readonly InMemoryReviewQueueRepository _queue = new();
        private readonly FakeUnitOfWork _unitOfWork = new();
        private readonly ReviewService _service;

        public ReviewServiceTests()
        {
            _service = new ReviewService(_queue, _slips, _unitOfWork);
        }

        private static ReviewEdit WonEdit(string slipId = "1001") => new(slipId, 1m, 2.58m, 2.63m, 3m);

        private ReviewItem Queue(int id, string slipId = "1001")
        {
            var item = new ReviewItem
            {
                Id = id,
                FileHash = $"hash{id}",
                FilePath = $"{id}.png",
                Extracted = new ExtractedSlip { SlipId = slipId, Stake = 1m, TotalOdds = 2.58m, Payout = null, BonusPercent = 3m }
            };
            _queue.Add(item);
            return item;
        }

        [Fact]
        public async Task Queue_is_returned()
        {
            Queue(1);
            Queue(2, "1002");

            var queue = await _service.GetQueueAsync();

            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public async Task Preview_derives_the_status_and_saves_nothing()
        {
            Queue(1);

            var preview = await _service.PreviewAsync(WonEdit());

            Assert.Equal(SlipStatus.Won, preview.Slip.Status);
            Assert.True(preview.CanAccept);
            Assert.Empty(_slips.Slips);
            Assert.Single(_queue.Items);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Preview_without_a_payout_is_undetermined()
        {
            var preview = await _service.PreviewAsync(WonEdit() with { Payout = null });

            Assert.Null(preview.Slip.Status);
            Assert.False(preview.CanAccept);
        }

        [Fact]
        public async Task Preview_with_a_zero_payout_is_Lost()
        {
            var preview = await _service.PreviewAsync(WonEdit() with { Payout = 0m });

            Assert.Equal(SlipStatus.Lost, preview.Slip.Status);
            Assert.True(preview.CanAccept);
        }

        [Fact]
        public async Task Preview_flags_a_saved_slip_reference_as_duplicate()
        {
            _slips.Add(new Slip { SlipId = "1001" });

            var preview = await _service.PreviewAsync(WonEdit("1001"));

            Assert.True(preview.IsDuplicate);
            Assert.False(preview.CanAccept);
        }

        [Fact]
        public async Task Preview_compares_the_slip_reference_exactly_including_case()
        {
            _slips.Add(new Slip { SlipId = "AbC==" });

            var preview = await _service.PreviewAsync(WonEdit("abc=="));

            Assert.False(preview.IsDuplicate);
        }

        [Fact]
        public async Task Zero_bonus_is_treated_as_no_bonus()
        {
            var preview = await _service.PreviewAsync(WonEdit() with { BonusPercent = 0m });

            Assert.Null(preview.Slip.BonusPercent);
        }

        [Fact]
        public async Task Accept_saves_the_slip_and_removes_the_item_in_one_save()
        {
            var item = Queue(1);

            var result = await _service.AcceptAsync(item.Id, WonEdit());

            Assert.Equal(ReviewOutcome.Saved, result.Outcome);
            var saved = Assert.Single(_slips.Slips);
            Assert.Equal("1001", saved.SlipId);
            Assert.Equal(2.63m, saved.Payout);
            Assert.Equal(SlipStatus.Won, saved.Status);
            Assert.Empty(_queue.Items);
            Assert.Equal(1, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Accept_only_removes_the_accepted_item()
        {
            Queue(1);
            Queue(2, "1002");

            await _service.AcceptAsync(1, WonEdit());

            Assert.Equal(2, Assert.Single(_queue.Items).Id);
        }

        [Fact]
        public async Task Accept_with_a_lost_edit_saves_a_zero_payout()
        {
            var item = Queue(1);

            var result = await _service.AcceptAsync(item.Id, WonEdit() with { Payout = 0m });

            Assert.Equal(ReviewOutcome.Saved, result.Outcome);
            var saved = Assert.Single(_slips.Slips);
            Assert.Equal(SlipStatus.Lost, saved.Status);
            Assert.Equal(0m, saved.Payout);
        }

        [Fact]
        public async Task Accept_that_is_still_undetermined_keeps_the_item_and_saves_nothing()
        {
            var item = Queue(1);

            var result = await _service.AcceptAsync(item.Id, WonEdit() with { Payout = null });

            Assert.Equal(ReviewOutcome.StillUndetermined, result.Outcome);
            Assert.Empty(_slips.Slips);
            Assert.Single(_queue.Items);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Accept_with_an_empty_slip_reference_keeps_the_item()
        {
            var item = Queue(1, "");

            var result = await _service.AcceptAsync(item.Id, WonEdit(slipId: "  "));

            Assert.Equal(ReviewOutcome.StillUndetermined, result.Outcome);
            Assert.Empty(_slips.Slips);
            Assert.Single(_queue.Items);
        }

        [Fact]
        public async Task Accept_of_a_saved_slip_reference_is_rejected_and_the_item_stays()
        {
            _slips.Add(new Slip { SlipId = "1001" });
            var item = Queue(1, "1001");

            var result = await _service.AcceptAsync(item.Id, WonEdit("1001"));

            Assert.Equal(ReviewOutcome.Duplicate, result.Outcome);
            Assert.Single(_slips.Slips);
            Assert.Single(_queue.Items);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Accept_of_a_missing_item_is_not_found()
        {
            var result = await _service.AcceptAsync(99, WonEdit());

            Assert.Equal(ReviewOutcome.NotFound, result.Outcome);
            Assert.Empty(_slips.Slips);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Discard_removes_the_item_without_saving_a_slip()
        {
            var item = Queue(1);

            var removed = await _service.DiscardAsync(item.Id);

            Assert.True(removed);
            Assert.Empty(_queue.Items);
            Assert.Empty(_slips.Slips);
            Assert.Equal(1, _unitOfWork.SaveCount);
        }

        [Fact]
        public async Task Discard_of_a_missing_item_returns_false()
        {
            var removed = await _service.DiscardAsync(99);

            Assert.False(removed);
            Assert.Equal(0, _unitOfWork.SaveCount);
        }
    }
}