using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Domain.Validation;

namespace MyBet.Application.Review
{
    public class ReviewService
    {
        private readonly IReviewQueueRepository _queue;
        private readonly ISlipRepository _slips;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IReviewQueueRepository queue, ISlipRepository slips, IUnitOfWork unitOfWork)
        {
            _queue = queue;
            _slips = slips;
            _unitOfWork = unitOfWork;
        }

        public Task<IReadOnlyList<ReviewItem>> GetQueueAsync(CancellationToken ct = default) =>
            _queue.GetAllAsync(ct);

        public async Task<ReviewPreview> PreviewAsync(ReviewEdit edit, CancellationToken ct = default)
        {
            var slip = Build(edit);
            var duplicate = slip.SlipId.Length > 0 && await _slips.ExistsAsync(slip.SlipId, ct);
            return new ReviewPreview(slip, duplicate);
        }

        public async Task<ReviewResult> AcceptAsync(int itemId, ReviewEdit edit, CancellationToken ct = default)
        {
            var item = await _queue.GetAsync(itemId, ct);
            if (item is null)
                return new ReviewResult(ReviewOutcome.NotFound, null);

            var preview = await PreviewAsync(edit, ct);
            if (preview.Slip.NeedsReview)
                return new ReviewResult(ReviewOutcome.StillUndetermined, preview.Slip);

            if (preview.IsDuplicate)
                return new ReviewResult(ReviewOutcome.Duplicate, preview.Slip);

            _slips.Add(preview.Slip.ToSlip());
            _queue.Remove(itemId);
            await _unitOfWork.SaveChangesAsync(ct);

            return new ReviewResult(ReviewOutcome.Saved, preview.Slip);
        }

        public async Task<bool> DiscardAsync(int itemId, CancellationToken ct = default)
        {
            var item = await _queue.GetAsync(itemId, ct);
            if (item is null) return false;

            _queue.Remove(itemId);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        private static ExtractedSlip Build(ReviewEdit edit)
        {
            var slip = new ExtractedSlip
            {
                SlipId = edit.SlipId.Trim(),
                Stake = edit.Stake,
                TotalOdds = edit.TotalOdds,
                Payout = edit.Payout,
                BonusPercent = edit.BonusPercent is 0m ? null : edit.BonusPercent
            };

            slip.Status = SlipStatusResolver.Resolve(slip);
            return slip;
        }
    }
}