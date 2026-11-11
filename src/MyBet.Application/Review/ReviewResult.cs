using MyBet.Domain.Entities;

namespace MyBet.Application.Review
{
    public record ReviewResult(ReviewOutcome Outcome, ExtractedSlip? Slip);
}