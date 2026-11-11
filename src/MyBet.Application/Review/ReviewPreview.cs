using MyBet.Domain.Entities;

namespace MyBet.Application.Review
{
    public record ReviewPreview(ExtractedSlip Slip, bool IsDuplicate)
    {
        public bool CanAccept => !Slip.NeedsReview && !IsDuplicate;
    }
}