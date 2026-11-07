using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;

namespace MyBet.Tests.Domain
{
    public class SlipStatusResolverTests
    {
        public static TheoryData<decimal?, decimal?, decimal?, decimal?, SlipStatus?> Cases => new()
        {
            { 1.00m, 2.58m, 3m, 2.63m, SlipStatus.Won },
            { 2.31m, 14.16m, 10m, 35.75m, SlipStatus.Won },
            { 1.00m, 2.92m, 3m, 2.98m, SlipStatus.Won },
            { 1.00m, 2.58m, null, 2.58m, SlipStatus.Won },
            { 2.00m, 5.00m, null, 0.00m, SlipStatus.Lost },
            { 2.00m, 5.00m, null, null, SlipStatus.Lost },
            { 2.00m, 5.00m, null, 4.10m, SlipStatus.CashedOut },
            { 2.00m, 5.00m, null, 12.00m, null },
            { null, 5.00m, null, 10.00m, null },
            { 2.00m, null, null, 10.00m, null },
            { null, null, null, null, null },
            { null, 5.00m, null, 0.00m, null },
        };

        [Theory]
        [MemberData(nameof(Cases))]
        public void Status_follows_the_money_numbers(
            decimal? stake, decimal? odds, decimal? bonus, decimal? payout, SlipStatus? expected)
        {
            var slip = new Slip
            {
                Stake = stake,
                TotalOdds = odds,
                BonusPercent = bonus,
                Payout = payout
            };

            Assert.Equal(expected, SlipStatusResolver.Resolve(slip));
        }
    }
}